package ontopt.pen;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class validates the grammar's syntax. 
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

public class GrammarValidator
{
    private Grammar grammar;

    private HashSet validated;

    public GrammarValidator(Grammar grammar)
    {
        this.grammar = grammar;

        validated = new HashSet();
    }

    public void validate() throws GrammarException
    {
        validate(grammar.PARSE_ROOT);
    }

    private void validate(Integer root) throws GrammarException
    {
        if (root.intValue() > Grammar.PHRASE_LOWER_LIMIT)
        {
            List rules = grammar.getAllRulesWithHead(root);
            if (rules == null || rules.isEmpty())
            {
                throw new GrammarException("Couldn't find rule for symbol: "
                		+ root + " (AKA " + grammar.getDataType(root)
                		+ ") in " + grammar.getGrammarFileName());
            }

            Rule rule;
            ArrayList body;
            Integer token;
            GrammarException ex;

            for (Iterator i = rules.iterator(); i.hasNext();)
            {
                rule = (Rule) i.next();

                if (validated.contains(rule))
                    continue;

                validated.add(rule);

                body = ((PhraseRule) rule).getBody();

                for (Iterator j = body.iterator(); j.hasNext();)
                {
                    token = (Integer) j.next();
                    validate(token);
                }
            }
        }
    }
}