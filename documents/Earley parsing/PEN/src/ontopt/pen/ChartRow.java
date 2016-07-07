package ontopt.pen;

import java.util.ArrayList;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class represents the row of a chart. 
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

public class ChartRow
{
    /**
     * The list of all chart rows
     */
    private ArrayList<ChartRow> stateList;

    /**
     * The list of rules that contributed to the present state of this row
     */
    private ArrayList<Integer> parents;

    /**
     * The process that created the row Predictor, Scanner or Completer
     */
    private String process;

    /**
     * The state of the row. A unique identifier
     */
    private Integer state;

    /**
     * The index of where the dot is located in reference to body of the rule
     */
    private int dot = 0;

    /**
     * points to the current rule being used
     */
    private Rule rule;

    /**
     * holds the position of the begining of the rule in index 0 holds the position of the dot in index 1
     */
    private int[] positions;

    /**
     * The constructor
     * 
     * @param pRule
     *            The rule used in this row
     */
    public ChartRow(Rule pRule)
    {
        this(pRule, new int[2]);
    }

    /**
     * The constructor
     * 
     * @param pRule
     *            The rule used in this row
     * @param pPositions
     *            An array where the first value corresponds to where the rule begins and the second to where
     *            the dot lies always in respect to the input sentence.
     */
    public ChartRow(Rule pRule, int[] pPositions)
    {
        parents = new ArrayList<Integer>();
        this.rule = pRule;
        positions = pPositions;
    }

    /**
     * Gets the row corresponding to the specified state
     * 
     * @param pState
     *            The state to look for
     * @return The row with the state
     */
    public ChartRow getChartRowFromState(Integer pState)
    {
        return (ChartRow) stateList.get(pState.intValue());
    }

    /**
     * Sets the name of the process
     * 
     * @param p
     *            The name to be set
     */
    protected void setProcess(String p)
    {
        process = p;
    }

    /**
     * Adds this row to the state list.
     * 
     * @param pStateList
     *            The global state list that is shared
     */
    protected void incState(ArrayList<ChartRow> pStateList)
    {
        this.stateList = pStateList;
        state = new Integer(this.stateList.size());
        stateList.add(this);
    }

    /**
     * Sets the position of the dot relative to the size of this rule
     * 
     * @param dot
     *            the position
     */
    protected void setDot(int dot)
    {
        this.dot = dot;
    }

    /**
     * Gets the position of the dot relative to the size of this rule
     * 
     * @return the position of the dot
     */
    public int getDot()
    {
        return dot;
    }

    /**
     * Gets the state of this row, the state is a unique id
     * 
     * @return The state of this row
     */
    public Integer getState()
    {
        return state;
    }

    /**
     * Adds a new parent to this row
     * 
     * @param s
     *            the parent to add
     */
    public void addParentState(Integer s)
    {
        parents.add(s);
    }

    /**
     * Adds a list of parents to this row
     * 
     * @param s
     *            the list of parents
     */
    public void addParentStates(ArrayList<Integer> s)
    {
        for (int i = 0; i < s.size(); i++)
        {
            if (!parents.contains(s.get(i)))
            {
                parents.add(s.get(i));
            }
        }
    }

    /**
     * Gets the list of parents
     * 
     * @return list of parents
     */
    public ArrayList<Integer> getParents()
    {
        return parents;
    }

    /**
     * gets the array corresponding to the position of where this rule begins and where the dot lies. These
     * values are in respect to the input sentence.
     * 
     * @return
     */
    public int[] getPositions()
    {
        return positions;
    }

    /**
     * Sets the array of positions where the first value corresponds to where the rule begins and the second
     * to where the dot lies always in respect to the input sentence.
     * 
     * @param positions
     *            the array of positions
     */
    protected void setPositions(int[] positions)
    {
        this.positions = positions;
    }

    /**
     * The rule associated to this row
     * 
     * @return The rule
     */
    public Rule getRule()
    {
        return rule;
    }

    /**
     * Check if this rule has been completely fired
     * 
     * @return true if it is complete, false otherwise
     */
    public boolean isComplete()
    {
        if (rule instanceof PhraseRule)
        {
            return (dot == ((PhraseRule) rule).body.size());
        }

        return true; // its a terminal
    }

    /**
     * Gets the next part of rule that needs to be fired
     * 
     * @return an Integer corresponding to the next part of the rule
     */
    public Integer getNextConstituent()
    {
        if (rule instanceof PhraseRule && !isComplete())
        {
            return (Integer) ((PhraseRule) rule).getBody().get(dot);
        }

        return null;
    }

    /**
     * Check if this row is equal to another.
     * 
     * @param row
     *            The row to compare to
     * @return true if they are equal, false otherwise
     */
    public boolean equals(ChartRow row)
    {
        return (row.rule.equals(this.rule) && row.positions[0] == this.positions[0] && row.positions[1] == this.positions[1] && this.getParents().equals(row.getParents()));
    }

    /**
     * gets the string representation of this row.
     * 
     * @return The string representation.
     */
    public String toString()
    {
        return state + " " + rule.toString() + " [" + positions[0] + " " + positions[1] + "]" + " " + getParents().toString() + " " + process + " " + dot;
    }
}