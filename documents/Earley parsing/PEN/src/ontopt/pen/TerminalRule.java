package ontopt.pen;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class represents a terminal rule, consisting of a terminal token.
 * </p>
 * <p>
 * Copyright: Copyright (c) 2002-2009
 * </p>
 * <p>
 * Company: CISUC
 * </p>
 * 
 * @author Nuno Seco
 * @version 1.0
 */

public class TerminalRule extends Rule
{
    /**
     * A string representation of the word associated to the terminal symbol
     */
    private String word;

    /**
     * The constructor
     * 
     * @param pHead
     *            The Head of the rule (terminal)
     * @param pWord
     *            The word associated
     * @param grammar
     *            The grammar that is being used.
     */
    public TerminalRule(Integer pHead, String pWord, Grammar grammar)
    {
        super(pHead, grammar);
        this.word = pWord;
    }

    /**
     * Gets the word associated to the terminal symbol
     * 
     * @return The word
     */
    public String getWord()
    {
        return word;
    }

    /**
     * Gets a string representation of this rule
     * 
     * @return The string
     */
    public String toString()
    {
        String s;
        s = grammar.getDataType(head) + " - " + word;

        return s;
    }

    /**
     * Checks if this rule is equal to the one being passed.
     * 
     * @param pRule
     *            The rule to check
     * @return true if they are equal, false otherwise
     */
    public boolean equals(Rule pRule)
    {
        if (pRule instanceof TerminalRule)
        {
            return (this.head.equals(pRule.head) && this.word.equals(((TerminalRule) pRule).word));
        }

        return false;
    }
}