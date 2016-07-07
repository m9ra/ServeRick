package ontopt.pen;

import java.io.OutputStream;
import java.io.PrintStream;
import java.util.Iterator;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class is used to print trees containing the derivation of sentences.
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

public class Outputter
{
    private PrintStream _out;

    public Outputter(OutputStream out)
    {
        _out = new PrintStream(out);
    }

    public void print(SemanticNode node, boolean printScores, boolean printWeights, int ident)
    {
    	_out.print(getIdent(ident));
    	_out.print((node.isLeaf() ? "> " : ""));
    	_out.print((printScores ? node.getScore() + ":" : ""));
    	_out.print("[" + node.label + "]");
    	_out.print((printWeights ? "(" + node.weight + ")" : ""));
    	_out.print(node.getAnnotation() != "" ? "(@ " + node.getAnnotation() + ")" : "");
    	_out.println();
    	_out.flush();

         ident++;
         for (Iterator i = node.getChildren().iterator(); i.hasNext();)
         {
             print((SemanticNode) i.next(), printScores, printWeights, ident);
         }
    }
    
    public void print(SemanticNode node, int ident)
    {
    	print(node, true, true, ident);
    }

    private String getIdent(int ident)
    {
        String buffer = "";
        for (int i = 0; i < ident; i++)
        {
            //buffer += "\t";
        	buffer += "   ";
        }
        return buffer;
    }
}
