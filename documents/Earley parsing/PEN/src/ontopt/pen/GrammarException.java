package ontopt.pen;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This exception is thrown when the grammar has syntax problems.
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

public class GrammarException extends Exception
{
    private String errorMessage;

    public GrammarException(String err)
    {
        errorMessage = err;
    }

    public String toString()
    {    	
        return errorMessage;
    }
}
