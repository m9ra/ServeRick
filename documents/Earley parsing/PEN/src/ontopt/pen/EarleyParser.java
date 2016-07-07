package ontopt.pen;

import java.io.BufferedReader;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This is the main class. It implements the Earley's chart parsing algorithm.
 * </p>
 * <p>
 * Copyright: Copyright (c) 2002-2009
 * </p>
 * <p>
 * Company: CISUC
 * </p>
 * 
 * @author Nuno Seco, Hugo Gon�alo Oliveira
 * @version 1.0
 */

public class EarleyParser
{
	private boolean stop;
	
	/**
	 * A dummy rule. This is the first rule to be put in the chart. it intializes the parsing process
	 */
	private PhraseRule dummieRule;

	/**
	 * The grammar that has the syntactic rules.
	 */
	private Grammar grammar;

	/**
	 * An array of charts. The n+1 charts, where n = number of words in sentence.
	 */
	private Chart[] chartArray;

	/**
	 * Time spent in parsing
	 */
	private long parseTime;
	
	/**
	 * The constructor
	 * 
	 * @throws GrammarException
	 */
	public EarleyParser(String grammarFile) throws GrammarException
	{
		grammar = new Grammar(grammarFile);
		new GrammarValidator(grammar).validate();
		dummieRule = new PhraseRule(0, "", null, Grammar.PARSE_ROOT, grammar);
		
		stop = false;
	}

	/**
	 * Receives a sentence to be parsed
	 * 
	 * @param sentence
	 *            The sentence to parse
	 * @return A list of possible parse trees.
	 */
	public synchronized ArrayList<SemanticNode> parseSentence(Sentence sentence)
	{
		stop = false;
		
		//long begin = System.currentTimeMillis();
		chartArray = new Chart[sentence.getSentenceSize() + 1];
		ArrayList<ChartRow> stateList = new ArrayList<ChartRow>();
	
		for (int i = 0; i < chartArray.length; i++)
		{			
			chartArray[i] = new Chart(stateList);
		}

		ChartRow row = new ChartRow(dummieRule);
		chartArray[0].addChartRow(row);
		
		//System.err.println("sentence size = "+sentence.getSentenceSize());
		for (int i = 0; i < sentence.getSentenceSize() + 1; i++)
		{
			if(stop)
				return null;
			
			//System.err.println("chart array ["+i+"] size= "+chartArray[i].size());
			for (int j = 0; j < chartArray[i].size(); j++)
			{
				if(stop)
					return null;
				
				row = chartArray[i].getChartRow(j);
				if (!row.isComplete() && row.getNextConstituent().compareTo(Grammar.PHRASE_LOWER_LIMIT) >= 0)
				{
					//System.err.println(1);
					predictor(row);
				}
				else if (!row.isComplete() && row.getNextConstituent().compareTo(Grammar.PHRASE_LOWER_LIMIT) < 0)
				{
					//System.err.println(2);
					scanner(row, sentence);
				}
				else
				{
					//System.err.println(3);
					completer(row);
				}
			}
		}
		
		//printChart();
		ArrayList<SemanticNode> trees = getTrees();
		//parseTime = System.currentTimeMillis() - begin;
		chartArray = null;
		return trees;
	}

	public void stopParsing()
	{
		stop = true;
	}
	
	public boolean isStopped()
	{
		return stop;
	}
	
	public Grammar getGrammar()
	{
		return grammar;
	}

	/**
	 * Get time spent in last parse
	 * 
	 * @return Time spent parsing
	 */
	public long getLastParseTime()
	{
		return parseTime;
	}

	/**
	 * Gets the parse trees associated to the current charts
	 * 
	 * @return list of parse trees
	 */
	public ArrayList<SemanticNode> getTrees()
	{
		ArrayList<ChartRow> ruleRoots = chartArray[chartArray.length - 1].getRoots();
		ArrayList<SemanticNode> sentenceRoots = new ArrayList<SemanticNode>();

		for (int i = 0; i < ruleRoots.size(); i++)
		{
			sentenceRoots.add(getTree((ChartRow) ruleRoots.get(i)));
		}

		Collections.sort(sentenceRoots);
		return sentenceRoots;
	}

	/**
	 * Recursive method to get parse trees. Creates TreeNodes from the chartrow and then recurses on the
	 * chartrow parents.
	 * 
	 * @param node
	 *            The chartrow
	 * @return The parse tree
	 */
	private SemanticNode getTree(ChartRow node)
	{
		ArrayList<Integer> parents = node.getParents();
		SemanticNode root;

		Rule rule = node.getRule();

		if (rule instanceof PhraseRule)
		{
			root = new SemanticNode(grammar.getDataType(rule.getHead()), rule.getWeight(), rule.getAnnotation());
		}
		else
		{
			root = new SemanticNode(((TerminalRule) rule).getWord(), rule.getWeight(), rule.getAnnotation());
		}

		for (int i = parents.size() - 1; i >= 0; i--)
		{
			root.addChild(getTree(node.getChartRowFromState((Integer) parents.get(i))));
		}

		return root;
	}

	/**
	 * The predictor Process. Creates new states representing top-down expectations generated during the
	 * parsing process. The Predictor is applied to any state that has a nonterminal to the right of the dot
	 * that is not a part of speech category. This application results in the ccreation of one new state for
	 * each alternative expansion of that nonterminal privided by the grammar. These new states are placed
	 * into the same chart entry as the generating state. They begin and end at the point in the input where
	 * the generating state ends.
	 * 
	 * @param row
	 *            The row to be predicted
	 */
	private void predictor(ChartRow row)
	{
		Integer next = row.getNextConstituent();
		ArrayList<Rule> list = grammar.getAllRulesWithHead(next);

		// System.out.println("LISTA: "+list);
		// System.out.println("ROW: "+row);

		ChartRow newRow;
		int[] positions = new int[2];
		positions[0] = row.getPositions()[1];
		positions[1] = positions[0];

		for (int i = 0; i < list.size(); i++)
		{
			newRow = new ChartRow((Rule) list.get(i), positions);
			newRow.setProcess("Predictor");
			enqueue(newRow, positions[0]);
		}
	}

	/**
	 * When a state has a part of speech category to the right of the dot, the scanner is called to examine
	 * the input and incorporate a state corresponding to the predicted part of speech into the chart. This is
	 * accomplisged by creating a new state from the input state with the dot advanced over the predicted
	 * input category.
	 * 
	 * @param row
	 *            the row to be scanned
	 * @param sentence
	 *            The sentence being parsed
	 */
	private void scanner(ChartRow row, Sentence sentence)
	{
		ChartRow newRow;
		int positions[] = new int[2];

		if (row.getPositions()[1] >= sentence.getSentenceSize())
		{                    
			if (row.getNextConstituent() != null && row.getNextConstituent().equals(Grammar.EMPTY_TERMINAL))
			{
				positions[0] = row.getPositions()[1];
				positions[1] = row.getPositions()[1];
				newRow = new ChartRow(new TerminalRule(row.getNextConstituent(), "", grammar), positions);
				newRow.setProcess("Scanner");
				enqueue(newRow, positions[1]);                                
			}
			return;
		}

		Integer next = row.getNextConstituent();
		String word = sentence.getWord(row.getPositions()[1]);

		if (grammar.getTerminal(word).equals(next) || next.equals(Grammar.UNKNOWN_TERMINAL))
		{
			positions[0] = row.getPositions()[1];
			positions[1] = row.getPositions()[1] + 1;
			newRow = new ChartRow(new TerminalRule(next, word, grammar), positions);
			newRow.setProcess("Scanner");
			enqueue(newRow, positions[1]);
		}

		if (next.equals(Grammar.EMPTY_TERMINAL))
		{
			positions[0] = row.getPositions()[1];
			positions[1] = row.getPositions()[1];
			newRow = new ChartRow(new TerminalRule(next, "", grammar), positions);
			newRow.setProcess("Scanner");
			enqueue(newRow, positions[1]);
		}
	}

	/**
	 * The completer is applied to a state when its dot has reached the right end of the rule. Intuitively,
	 * the presence of such a state represents the fact that the parser has successfully discovered a
	 * prticualar grammatical category over some span of the input. The purpose of the completer is to find
	 * and advance all previously created states that were looking for this grammatical category at this
	 * position int he input. New states are then created by copying the older state, advancing the dot over
	 * the expected category and installing the new state in the current chart entry.
	 * 
	 * @param row
	 *            The row of the chart
	 */
	private void completer(ChartRow row)
	{
		int chartIndex = row.getPositions()[0];
		ChartRow newRow;
		int positions[];
		for (int i = 0; i < chartArray[chartIndex].size(); i++)
		{

			if (chartArray[chartIndex].getChartRow(i).getPositions()[1] == chartIndex && !chartArray[chartIndex].getChartRow(i).isComplete()
					&& chartArray[chartIndex].getChartRow(i).getNextConstituent().equals(row.getRule().getHead()))
			{
				positions = new int[2];

				positions[0] = chartArray[chartIndex].getChartRow(i).getPositions()[0];
				positions[1] = row.getPositions()[1];
				newRow = new ChartRow(chartArray[chartIndex].getChartRow(i).getRule(), positions);
				newRow.addParentState(row.getState());
				newRow.addParentStates(chartArray[chartIndex].getChartRow(i).getParents());
				newRow.setProcess("Completer");
				newRow.setDot(chartArray[chartIndex].getChartRow(i).getDot() + 1);
				enqueue(newRow, row.getPositions()[1]);
			}
		}
	}

	/**
	 * Adds the chartrow to the chart if it does not already exist.
	 * 
	 * @param row
	 *            the row to add
	 * @param index
	 *            the index of the chart
	 */
	private void enqueue(ChartRow row, int index)
	{
		if (row.getRule().getHead() == null)
		{
			return;
		}

		if (!chartArray[index].exists(row))
		{
			chartArray[index].addChartRow(row);

		}
	}

	/**
	 * Prints the chart
	 */
	private void printChart()
	{
		for (int i = 0; i < chartArray.length; i++)
		{
			System.out.println("Chart " + i);
			System.out.println(chartArray[i].toString());
		}
	}

	public static void main(String[] args)
	{
		if (args.length != 2)
		{
			System.out.println("Utilizacao:\n");
			System.out.println("java -jar pen.jar <gramatica> <ficheiro_frases>");
			return;
		}

		try
		{
			EarleyParser parser = new EarleyParser(args[0]);
			SemanticNode node;
			Outputter outputter = new Outputter(System.out);
			ArrayList<SemanticNode> parses;
			String buffer;

			BufferedReader reader = new BufferedReader(new FileReader(args[1]));

			while ((buffer = reader.readLine()) != null)
			{
				//if(!buffer.startsWith("#"))
				//{
					System.out.println("\n***** Derivacoes para: \n" + buffer);
					System.out.println("");
					parses = parser.parseSentence(new PenSentence(buffer));
					for (int i = 0; i < parses.size(); i++)
					{
						node = (SemanticNode) parses.get(i);
						outputter.print(node, true, false, 0);
					}
				//}

			}

		}
		catch (FileNotFoundException e)
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		catch (IOException e)
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		catch (GrammarException e)
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

}