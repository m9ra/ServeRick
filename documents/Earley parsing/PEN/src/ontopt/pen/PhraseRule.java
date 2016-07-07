package ontopt.pen;

import java.util.ArrayList;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class represents the rule of a grammar, including its body.
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

public class PhraseRule extends Rule
{
    /**
     * Represents the body of the rule. e.g. HEAD - B1, B2, ..., Bn Everything to the right of - is the body.
     */
    protected ArrayList<Integer> body;

    /**
     * The Constructor
     * 
     * @param pHead
     *            The head of the rule
     * @param pBody
     *            The body of the rule
     * @param pGrammar
     *            The Grammar containing the rule
     */
    public PhraseRule(Integer pWeight, String pAnnotation, Integer pHead, ArrayList<Integer> pBody, Grammar pGrammar)
    {
        super(pWeight, pAnnotation, pHead, pGrammar);
        this.body = pBody;
    }

    /**
     * The Constructor
     * 
     * @param pHead
     *            The head of the rule
     * @param body
     *            The body of the rule
     * @param pGrammar
     *            The grammar containing the rule
     */
    public PhraseRule(Integer pWeight, String pAnnotation, Integer pHead, Integer body, Grammar pGrammar)
    {
        super(pWeight, pAnnotation, pHead, pGrammar);
        this.body = new ArrayList<Integer>();
        this.body.add(body);
    }

    /**
     * Gets the list representing the body
     * 
     * @return The list of the body
     */
    public ArrayList<Integer> getBody()
    {
        return body;
    }

    /**
     * Sets the body of the rule
     * 
     * @param pBody
     */
    public void setBody(ArrayList<Integer> pBody)
    {
        this.body = pBody;
    }

    /**
     * gets the string representation of this rule.
     * 
     * @return The string representation
     */
    public String toString()
    {
        String s = new String();
        s += grammar.getDataType(head) + " - ";

        for (int i = 0; i < body.size(); i++)
        {
            s += grammar.getDataType((Integer) body.get(i)) + " ";

        }
        return s;
    }

    /**
     * Checks if this rule is equal to another.
     * 
     * @param pRule
     *            The other rule
     * @return true if they are equal, false otherwise
     */
    public boolean equals(Rule pRule)
    {
        if (pRule instanceof PhraseRule && this.head != null)
        {
            return (this.head.equals(pRule.head) && this.body.equals(((PhraseRule) pRule).body));
        }

        return false;
    }
}