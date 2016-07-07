package ontopt.pen;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class represents a rule in the grammar.
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

public abstract class Rule
{

    public static final int DEFAULT_WEIGHT = 0;

    protected String annotation;

    /**
     * An Integer representing the rule weight
     */
    protected Integer weight;

    /**
     * An Integer representing the phrase type of this rule. e.g. HEAD - B1, B2, ..., Bn
     */
    protected Integer head;

    /**
     * The grammar that contains this rule
     */
    protected Grammar grammar;

    /**
     * The Constructor
     * 
     * @param head
     *            The head of the rule
     * @param grammar
     *            The grammar that contains this rule
     */
    public Rule(Integer weight, String annotation, Integer head, Grammar grammar)
    {
        this.weight = weight;
        this.annotation = annotation;
        this.head = head;
        this.grammar = grammar;
    }

    /**
     * The Constructor
     * 
     * @param head
     *            The head of the rule
     * @param grammar
     *            The grammar that contains this rule
     */
    public Rule(Integer head, Grammar grammar)
    {
        this.weight = DEFAULT_WEIGHT;
        this.annotation = "";
        this.head = head;
        this.grammar = grammar;
    }

    /**
     * Gets the Integer representation if this rule.
     * 
     * @return The Integre Representation of this rule.
     */
    public Integer getHead()
    {
        return head;
    }

    /**
     * Sets the integer reprsentation of this rule.
     * 
     * @param pHead
     *            The integer representation of this rule.
     */
    public void setHead(Integer pHead)
    {
        this.head = pHead;
    }

    /**
     * An abstract method that forces all inheriting classes to implement this method. This method should
     * return true if this rule and the passed rule are equal.
     * 
     * @param pRule
     *            The rule to compare to
     * @return true if they are equal, false otherwise
     */
    public abstract boolean equals(Rule pRule);

    public Integer getWeight()
    {
        return weight;
    }

    public String getAnnotation()
    {
        return annotation;
    }
}