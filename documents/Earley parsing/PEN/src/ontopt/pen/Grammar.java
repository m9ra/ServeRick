package ontopt.pen;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class represents a grammar with the processing rules, read from a text file.
 * </p>
 * <p>
 * Copyright: Copyright (c) 2002-2009
 * </p>
 * <p>
 * Company: CISUC
 * </p>
 * 
 * @author Nuno Seco, Hugo Gonçalo Oliveira
 * @version 1.0
 */

public class Grammar
{
    /**
     * The ID for a sentence. All Phrase types that are in upper case in the grammar file wiil be attributed a
     * unique ID.
     */
    public static final Integer PARSE_ROOT = new Integer(9999);

    public static final Integer UNKNOWN_TERMINAL = new Integer(-1);

     public static final Integer EMPTY_TERMINAL = new Integer(-2);

    /**
     * The begining ID number for new phrases.
     */
    public static final Integer PHRASE_LOWER_LIMIT = new Integer(8000);

    /**
     * The maximum number of phrase types that the parser can deal with,
     */
    public static final Integer VARIABLES_MAX_NUMBER = new Integer(1998);

    /**
     * The symbol that allows for comments inside the grammar file
     */
    public static final char COMMENT = '[';
    
    /**
     * The symbol that allows the definition of weights
     */
    //public static final String WEIGHT_SEPARATOR = "<#>";
    public static final String WEIGHT_SEPARATOR = "#";

    /**
     * The symbol that allows the definition of rules
     */
    public static final String ANTECEDENT_SEPARATOR = "::=";

    public static final String CONSEQUENT_SEPARATOR = "<&>";

    /**
     * The symbol that allows the definition of annotations
     */
    public static final String ANNOTATION_SEPARATOR = "<@>";

    /**
     * The symbol that allows including more grammar files
     */
    public static final String INCLUDE_INDICATOR = ">";

    /**
     * A Hashmap for holding the mappings between the phrases and their Integer ID.
     */
    private HashMap<String, Integer> variables;

    private HashMap<String, Integer> terminals;

    /**
     * The next variable to attributed to a new phrase type. It is initialized with (PHRASE_LOWER_LIMIT +
     * VARIABLES_MAX_NUMBER)
     */
    private int nextVariableID;

    private int nextTerminalID;

    /**
     * A hashmap that holds the grammar rules in the Integer ID format. Each key corresponds to a head of a
     * rule. The value is a list of all possible bodies for the head.
     */
    private HashMap<Integer, ArrayList<Rule>> grammar;

    private LinkedList<String> includedFiles;

    /**
     * A map that holds a mapping between Symbols and the bodies (of rules) in which it participates
     */
    private HashMap<Integer, ArrayList<Rule>> invertedGrammar;

    /**
     * The file where this grammar is written
     */
    private String grammarFile;
    
    /**
     * The constructor
     */
    public Grammar(String grammarFile)
    {
    	this.grammarFile = grammarFile;
    	
        grammar = new HashMap<Integer, ArrayList<Rule>>();
        invertedGrammar= new HashMap<Integer, ArrayList<Rule>>();
        includedFiles = new LinkedList<String>();
        variables = new HashMap<String, Integer>();
        variables.put("RAIZ", PARSE_ROOT);
        terminals = new HashMap<String, Integer>();
        terminals.put("<?>", UNKNOWN_TERMINAL);
        terminals.put("<>", EMPTY_TERMINAL);
        nextVariableID = PHRASE_LOWER_LIMIT.intValue() + VARIABLES_MAX_NUMBER.intValue();
        nextTerminalID = 1;
        readGrammar(grammarFile, new LinkedList<String>());
    }

    public String getGrammarFileName()
    {
    	return grammarFile;
    }
    
    /**
     * Transforms an Integer ID in the corresponding String representation.
     * 
     * @param token
     *            The Integer to transform
     * @return The string representation
     */
    public String getDataType(Integer token)
    {
        Iterator<String> it;
        Integer id;
        String s;
        if (token == null)
        {
            return "";
        }

        if (token.compareTo(PHRASE_LOWER_LIMIT) > 0)
        {
            it = variables.keySet().iterator();

            while (it.hasNext())
            {
                s = (String) it.next();
                id = (Integer) variables.get(s);
                if (id.equals(token))
                {
                    return s;
                }
            }
        }

        it = terminals.keySet().iterator();

        while (it.hasNext())
        {
            s = (String) it.next();
            id = (Integer) terminals.get(s);
            if (id.equals(token))
            {
                return s;
            }
        }

        return null;
    }

    /**
     * Get all rules that begin with the specified Head. A simple lookup in the hashtable.
     * 
     * @param head
     *            The head to lookup
     * @return A list of bodies beginning with that rule.
     */
    public ArrayList<Rule> getAllRulesWithHead(Integer head)
    {
        return grammar.get(head);
    }

    public ArrayList<Rule> getAllRulesWithHead(String head)
    {
        return grammar.get(getDataType(head));
    }

    public Integer getTerminal(String word)
    {
        Integer terminal;
        terminal = (Integer) terminals.get(word);

        if (terminal == null)
            return UNKNOWN_TERMINAL;

        return terminal;

    }

    public ArrayList<Rule> getRulesContaining(Integer symbol)
    {
        return invertedGrammar.get(symbol);
    }

    public ArrayList<Rule> getRulesContaining(String symbol)
    {
        Integer integer = getDataType(symbol);
        if (integer != null)
            return getRulesContaining(integer);

        return null;
    }

    public Set<String> getAllTerminals()
    {
        return terminals.keySet();
    }

    /**
     * A String representation of this class. It Returns a string in a chart like format with all the rules.
     * 
     * @return The string representation
     */
    public String toString()
    {
        String s = new String();
        Iterator<Integer> it = grammar.keySet().iterator();

        while (it.hasNext())
        {
            s += grammar.get(it.next()).toString() + "\n";
        }

        return s;
    }

    /**
     * Adds a rule into the grammar hashmap. It first checks if the head already exists and simply adds a new
     * rule to the list. Otherwise it creates a list with the rule.
     * 
     * @param rule
     *            the rule to be added
     */
    private void addRule(Rule rule)
    {
        ArrayList<Rule> rules;

        if ((rules = grammar.get(rule.getHead())) == null)
        {
            rules = new ArrayList<Rule>();
        }

        rules.add(rule);
        grammar.put(rule.getHead(), rules);
    }

    private void addRuleToInvertedGrammar(PhraseRule rule)
    {
        ArrayList<Rule> rules;
        ArrayList<Integer> body = rule.getBody();
        Integer element;

        for (Iterator<Integer> i = body.iterator(); i.hasNext();)
        {
            element = i.next();
            if ((rules = invertedGrammar.get(element)) == null)
            {
                rules = new ArrayList<Rule>();
            }

            rules.add(rule);
            invertedGrammar.put(element, rules);
        }
    }

    /**
     * Loads the grammar file into memory.
     */
    private void readGrammar(String grammarFile, LinkedList<String> toInclude)
    {
        String line;
        try
        {
            File file = new File(grammarFile);
            BufferedReader reader = new BufferedReader(new FileReader(file));

            while ((line = reader.readLine()) != null)
            {
                line = line.trim();
                if (line.equals("") || line.charAt(0) == COMMENT)
                {
                    continue;
                }
                else if (line.startsWith(INCLUDE_INDICATOR))
                {
                    String includedFile = file.getParent()
                    	+ File.separator
                    	+ line.replace(INCLUDE_INDICATOR, "").trim();

                    if (!includedFiles.contains(includedFile))
                    {
                        toInclude.add(includedFile);
                        includedFiles.add(includedFile);
                    }

                    continue;
                }

                addRule(line);
            }

            reader.close();

            if (!toInclude.isEmpty())
                readGrammar(toInclude.removeFirst(), toInclude);
        }
        catch (FileNotFoundException ex)
        {
            ex.printStackTrace();
        }
        catch (IOException ex)
        {
            ex.printStackTrace();
        }
    }

    /**
     * Transforms a String representation of a rule into a list representation with Integer Ids, then calls
     * addRule with the new list representation.
     * 
     * @param line
     *            The string to be transformed
     */
    private void addRule(String buffer)
    {
    	//System.out.println(buffer);
    	
        Integer weight = Rule.DEFAULT_WEIGHT;
        String annotation = "";
        String rule = buffer;
        Pattern p;
        Matcher m;

        p = Pattern.compile("(.*)" + WEIGHT_SEPARATOR + "(.*)" + ANNOTATION_SEPARATOR + "(.*)");
        m = p.matcher(buffer);
        if (m.matches())
        {
        	/*System.out.print("1: "+m.group(1)+" ; ");
        	System.out.print("2: "+m.group(2)+" ; ");
        	System.out.println("3: "+m.group(3));*/
        	
        	weight = Integer.parseInt(m.group(1).trim());
        	rule = m.group(2).trim();
            annotation = m.group(3).trim();
        }
        else
        {
            p = Pattern.compile("(.*) " + WEIGHT_SEPARATOR + "(.*)");
            m = p.matcher(buffer);
            if (m.matches())
            {
            	/*System.out.print("1: "+m.group(1)+" ; ");
            	System.out.println("2: "+m.group(2));*/
            	
                weight = Integer.parseInt(m.group(1).trim());
                rule = m.group(2).trim();
            }
            else
            {
                p = Pattern.compile("(.*) " + ANNOTATION_SEPARATOR + "(.*)");
                m = p.matcher(buffer);
                if (m.matches())
                {
                	/*System.out.print("1: "+m.group(1)+" ; ");
                	System.out.println("2: "+m.group(2));*/
                	
                    rule = m.group(1).trim();
                    annotation = m.group(2).trim();
                }
                else
                {
                    rule = buffer;
                }
            }
        }

        String[] st = rule.split("(" + ANTECEDENT_SEPARATOR + ")|(" + CONSEQUENT_SEPARATOR + ")");
        ArrayList<Integer> body = new ArrayList<Integer>();
        Integer token = null;

        Integer head = getDataType(st[0].trim());

        for (int i = 1; i < st.length; i++)
        {
            token = getDataType(st[i].trim());
            body.add(token);
        }

        PhraseRule phraseRule = new PhraseRule(weight, annotation, head, body, this);
        addRule(phraseRule);

        addRuleToInvertedGrammar(phraseRule);
    }

    /*private void addRule(String buffer)
    {
        Integer weight = Rule.DEFAULT_WEIGHT;
        String annotation = "";
        String rule = buffer;
        Pattern p;
        Matcher m;

        p = Pattern.compile("(.*) " + WEIGHT_SEPARATOR + "(.*)" + ANNOTATION_SEPARATOR + "(.*)");
        m = p.matcher(buffer);
        if (m.matches())
        {
            rule = m.group(1).trim();
            weight = Integer.parseInt(m.group(2).trim());
            annotation = m.group(3).trim();
        }
        else
        {
            p = Pattern.compile("(.*) " + WEIGHT_SEPARATOR + "(.*)");
            m = p.matcher(buffer);
            if (m.matches())
            {
                rule = m.group(1).trim();
                weight = Integer.parseInt(m.group(2).trim());
            }
            else
            {
                p = Pattern.compile("(.*) " + ANNOTATION_SEPARATOR + "(.*)");
                m = p.matcher(buffer);
                if (m.matches())
                {
                    rule = m.group(1).trim();
                    annotation = m.group(2).trim();
                }
                else
                {
                    rule = buffer;
                }
            }
        }

        String[] st = rule.split("(" + ANTECEDENT_SEPARATOR + ")|(" + CONSEQUENT_SEPARATOR + ")");
        ArrayList<Integer> body = new ArrayList<Integer>();
        Integer token = null;

        Integer head = getDataType(st[0].trim());

        for (int i = 1; i < st.length; i++)
        {
            token = getDataType(st[i].trim());
            body.add(token);
        }

        PhraseRule phraseRule = new PhraseRule(weight, annotation, head, body, this);
        addRule(phraseRule);

        addRuleToInvertedGrammar(phraseRule);
    }*/
    
    /**
     * Transforms a string representation into its corresponding ID.
     * 
     * @param token
     *            The string representation
     * @return The corresponding Integer ID.
     */
	private Integer getDataType(String token)
	{
		Integer id;
		String upperCaseToken = token.toUpperCase();
		
		//if (Character.isUpperCase(token.charAt(0)))
		if (Character.isUpperCase(token.charAt(0)) && token.equals(upperCaseToken))
		{
			if (variables.containsKey(token))
			{
				return variables.get(token);
			}

			variables.put(token, nextVariableID);
			id = nextVariableID;
			nextVariableID--;
			return id;
		}
		else
		{
			if ((id = (Integer) terminals.get(token)) == null)
			{
				terminals.put(token, nextTerminalID);
				id = nextTerminalID;
				nextTerminalID++;
			}
		}
		return id;
	}
    /*private Integer getDataType(String token)
    {
        Integer id;

        if (Character.isUpperCase(token.charAt(0)))
        {
            if (variables.containsKey(token))
            {
                return variables.get(token);
            }

            variables.put(token, nextVariableID);
            id = nextVariableID;
            nextVariableID--;

            if (nextVariableID == PHRASE_LOWER_LIMIT)
                System.err.println("VARIABLE OVERFLOW");

            return id;
        }
        else
        {
            if ((id = (Integer) terminals.get(token)) == null)
            {
                terminals.put(token, nextTerminalID);
                id = nextTerminalID;
                nextTerminalID++;

                if (nextTerminalID == PHRASE_LOWER_LIMIT)
                    System.err.println("TERMINAL OVERFLOW");

            }
        }
        return id;
    }*/
}

/**
 * int index = line.indexOf(WEIGHT_SEPARATOR); if (index > 0 && index < line.length()) { weight =
 * Integer.parseInt(line.substring(0, index).trim()); line = line.substring(index + WEIGHT_SEPARATOR.length() +
 * 1); } else { weight = Rule.DEFAULT_WEIGHT; }
 * 
 * index = line.indexOf(ANNOTATION_SEPARATOR); if (index > 0 && index < line.length()) { annotation =
 * line.substring(0, index).trim(); line = line.substring(index + ANNOTATION_SEPARATOR.length() + 1); }
 * 
 * 
 */
