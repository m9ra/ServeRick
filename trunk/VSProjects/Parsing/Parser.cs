using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace Parsing
{
    /// <summary>
    /// Provides parsing input texts according to context free grammar specification
    /// Uses modificated earley parser algorithm
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Grammar specifiing parsing rules
        /// </summary>
        private readonly GrammarBase _grammar;

        /// <summary>
        /// Translations from grammar sequence to corresponding active labels
        /// </summary>
        private readonly Dictionary<GrammarSequence, ActiveLabel> _activeChains = new Dictionary<GrammarSequence, ActiveLabel>();

        /// <summary>
        /// Initial transitions for parsing
        /// </summary>
        private readonly ActiveLabel[] _rootTransitions;

        /// <summary>
        /// Earley's agenda used as queue for not processed complete edges
        /// </summary>
        private readonly Queue<CompleteEdge> _agenda = new Queue<CompleteEdge>();

        /// <summary>
        /// Private buffer for holding of all reachable interpretations of input
        /// Is filled by reading interpretations of every next word context
        /// </summary>
        private readonly List<CompleteEdge> _interpretations = new List<CompleteEdge>();

        /// <summary>
        /// Contexts for reading next input interpretations
        /// </summary>
        private readonly HashSet<SourceContext> _nextWordContexts = new HashSet<SourceContext>();

        /// <summary>
        /// Storage for result
        /// </summary>
        private CompleteEdge _result;

        /// <summary>
        /// Creates parser, from given grammar
        /// (Accepts all CFG forms)
        /// </summary>
        /// <param name="grammar">Grammar specifing parsing rules</param>
        public Parser(GrammarBase grammar)
        {
            _grammar = grammar;
            grammar.Build();
            var nonTerminals = grammar.CollectNonTerminals(grammar.Root);

            //prepare edges
            foreach (var nonTerm in nonTerminals)
            {
                foreach (var sequence in nonTerm.Rule.Sequences)
                {
                    var activeLabel = new ActiveLabel(sequence);
                    _activeChains.Add(sequence, activeLabel);
                }
            }

            buildEdgeTransitions();

            //set root transitions
            _rootTransitions = getRootTransitions().ToArray();
        }

        /// <summary>
        /// Run parsing according to parser grammar on given text
        /// TODO: Error reporting
        /// </summary>
        /// <param name="text">Text input for parsing</param>
        /// <returns>AST Tree node on success, null otherwise</returns>
        public Node Parse(string text)
        {
            var w = Stopwatch.StartNew();
            var source = new Source(text);

            //initialize
            _agenda.Clear();
            _interpretations.Clear();
            _nextWordContexts.Clear();
            _result = null;

            var sourceData = new SourceData(source, _grammar);
            var startContext = source.CreateStartContext(sourceData);
            startContext.AddSelfEdgesFrom(_rootTransitions);

            _nextWordContexts.Add(startContext);

            while (_result == null)
            {
                if (!scan(sourceData))
                    break;

                processAgenda(sourceData);
            }

            w.Stop();
            Console.WriteLine("{0}ms", w.ElapsedMilliseconds);
            return buildOutput(_result);
        }


        #region Parsing algorigthm (TODO: will be heavily modified and refactored)

        private void processAgenda(SourceData sourceData)
        {
            while (_agenda.Count > 0)
            {
                var constituent = _agenda.Dequeue();
                var incommingEdges = constituent.StartContext.IncommingEdges;
                var extensibleEdges = incommingEdges.ExtensibleWith(constituent);

                foreach (var edge in extensibleEdges)
                {
                    if (edge.Label.WillComplete)
                    {
                        complete(sourceData, constituent, edge);
                    }
                    else
                    {
                        predict(sourceData, constituent, edge);
                    }
                }
            }
        }

        private static void predict(SourceData sourceData, CompleteEdge constituent, ActiveEdge edge)
        {
            //TODO check if new possible interpretation of input is added
            var newEdge = edge.ExtendBy(constituent);
            if (sourceData.Connect(newEdge))
            {
                foreach (var transition in newEdge.Label.Transitions)
                {
                    var transitionEdge = new ActiveEdge(transition, newEdge);
                    sourceData.Connect(transitionEdge);
                }
            }
        }

        private void complete(SourceData sourceData, CompleteEdge constituent, ActiveEdge edge)
        {
            //new constituent will be created
            var newConstituent = edge.CompleteBy(constituent);

            if (sourceData.Connect(newConstituent))
            {
                //constituent doesn't exists in the graph yet
                _agenda.Enqueue(newConstituent);
            }

            if (newConstituent.IsFinal && newConstituent.EndContext.EOF && newConstituent.StartContext.BOF)
            {
                _result = newConstituent;
                //prevent further processing
                _agenda.Clear();
            }

        }

        private bool scan(SourceData sourceData)
        {
            //read input interpretations
            _interpretations.Clear();
            foreach (var nextWordContext in _nextWordContexts)
            {
                //read all possible terminal types as Terminal matches
                var wordInterpretations = nextWordContext.GetInterpretations();

                _interpretations.AddRange(wordInterpretations);
            }

            if (_interpretations.Count == 0)
            {
                return false;
            }

            //fill agenda with discovered interpretations
            //prepare next word contexts for reading input
            _nextWordContexts.Clear();
            foreach (var interpretation in _interpretations)
            {
                _agenda.Enqueue(interpretation);
                sourceData.Connect(interpretation);

                if (_interpretations.Count < 2 || interpretation.EndContext != interpretation.StartContext)

                    _nextWordContexts.Add(interpretation.EndContext);
            }

            //cleanup
            /*if (_interpretations.Count > 10)
                        {
                            for (int i = 0; i < _interpretations.Count; ++i)
                            {
                                var interpretation = _interpretations[i];
                                if (interpretation.EndContext == interpretation.StartContext)
                                {
                                    _interpretations.RemoveAt(i);
                                    i--;
                                }        
                            }
                        }*/

            return true;
        }

        private Node buildOutput(CompleteEdge result)
        {
            if (result == null)
                return null;
            var root = new Node(_grammar, result);

            return root;
        }

        private void print(CompleteEdge edge)
        {
            if (edge == null)
                return;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(edge);
            print(edge.ExtendingEdge);
            var current = edge.ExtendedEdge;
            while (current != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(current);
                print(edge.ExtendingEdge);
                current = current.ExtendedEdge;
            }
        }

        private HashSet<ActiveLabel> getRootTransitions()
        {
            var rootTransitions = new HashSet<ActiveLabel>();
            foreach (var sequence in _grammar.Root.Rule.Sequences)
            {
                var activeLabel = _activeChains[sequence];
                rootTransitions.Add(activeLabel);
                rootTransitions.UnionWith(activeLabel.Transitions);
            }
            return rootTransitions;
        }

        private void buildEdgeTransitions()
        {
            foreach (var active in _activeChains.Values)
            {
                var currentActive = active;
                while (currentActive != null)
                {
                    var transitions = getTrasintions(currentActive);

                    currentActive.SetTransitions(transitions);
                    currentActive = currentActive.NextInChain;
                }
            }
        }

        private IEnumerable<ActiveLabel> getTrasintions(ActiveLabel label)
        {
            var transitions = new HashSet<ActiveLabel>();
            fillTransitions(label, transitions);

            return transitions;
        }

        private void fillTransitions(ActiveLabel label, HashSet<ActiveLabel> transitions)
        {
            var transitionNonTerm = label.CurrentTerm as NonTerminal;

            if (transitionNonTerm == null)
                //transitions are processed only on non terminals
                return;

            foreach (var sequence in transitionNonTerm.Rule.Sequences)
            {
                var transition = _activeChains[sequence];

                if (transitions.Add(transition))
                {
                    //recursive add transitions of added edge
                    fillTransitions(transition, transitions);
                }
            }
        }
        #endregion
    }
}
