package ontopt.pen;

import java.util.ArrayList;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class represents a chart in the Earley algorithm. 
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

public class Chart
{
    /**
     * The chart used for storing previous computations
     */
    private ArrayList<ChartRow> chart;

    /**
     * A list that all chartrows from all charts share. This list used for constructing the parse tree. The
     * original earley algorithm is a membership algorithm and not a parsing algorithm.
     */
    private ArrayList<ChartRow> stateList;

    /**
     * The constructor
     * 
     * @param pStateList
     *            The stateList that is common to all chartrows
     */
    public Chart(ArrayList<ChartRow> pStateList)
    {
        this.stateList = pStateList;
        chart = new ArrayList<ChartRow>();
    }

    /**
     * Adds a new row to this chart
     * 
     * @param row
     *            The row to add
     */
    public void addChartRow(ChartRow row)
    {
        row.incState(stateList);
        chart.add(row);
    }

    /**
     * Gets the size of the chart.
     * 
     * @return the size of the chart
     */
    public int size()
    {
        return chart.size();
    }

    /**
     * Gets the row at the specified index
     * 
     * @param index
     *            The specified index
     * @return The row
     */
    public ChartRow getChartRow(int index)
    {
        return (ChartRow) chart.get(index);
    }

    /**
     * Checks if the specified row exists in the chart
     */
    public boolean exists(ChartRow row)
    {
        for (int i = chart.size() - 1; i >= 0; i--)
        {
            if (((ChartRow) chart.get(i)).equals(row))
            {
                return true;
            }
        }

        return false;
    }

    /**
     * Returns a string representation of this chart
     * 
     * @return The string representation
     */
    public String toString()
    {
        String s = new String();
        for (int i = 0; i < chart.size(); i++)
        {
            s += chart.get(i).toString() + "\n";
        }
        return s;
    }

    /**
     * Gets the rows which have succeeded in parsing the sentence. These rows correspond to the roots of parse
     * tree.
     * 
     * @return A list rows
     */
    public ArrayList<ChartRow> getRoots()
    {
        ChartRow cw;
        ArrayList<ChartRow> roots = new ArrayList<ChartRow>();

        for (int i = 0; i < chart.size(); i++)
        {
            cw = (ChartRow) chart.get(i);
            if (cw.getRule().getHead() != null && cw.getRule().getHead().equals(Grammar.PARSE_ROOT) && cw.isComplete() && cw.getPositions()[0] == 0)
            {
                roots.add(cw);
            }
        }

        return roots;
    }
}