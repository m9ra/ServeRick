package ontopt.pen;

import java.text.BreakIterator;
import java.util.ArrayList;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * Original Sentence class.
 * </p>
 * <p>
 * Copyright: Copyright (c) 2002-2009
 * </p>
 * <p>
 * Company: CISUC
 * </p>
 */

public class PenSentence extends Sentence
{
    private ArrayList<String> _sentence;

    public PenSentence()
    {
        _sentence = new ArrayList<String>();
    }

    public PenSentence(String sentence)
    {
        this();
        tokenize(sentence);
    }

    public void setSentence(String sentence)
    {
        tokenize(sentence);
    }

    protected void tokenize(String sentence)
    {
        BreakIterator it = BreakIterator.getWordInstance();
        it.setText(sentence);

        String current;
        int start = it.first();
        for (int end = it.next(); end != BreakIterator.DONE; start = end, end = it.next())
        {
            current = sentence.substring(start, end).trim();
            if (!current.equals(""))
            {
                _sentence.add(current);
            }
        }
    }

    /**
     * Returns the word at the specified index
     * 
     * @param index
     *            the index to look up
     * @return The word at the specified index
     */

    public String getWord(int index)
    {
        return _sentence.get(index);
    }

    public int getSentenceSize()
    {
        return _sentence.size();
    }

    public int getIndex(String token)
    {
        return _sentence.indexOf(token);
    }

    public PenSentence getSubsequence(int startIndex)
    {
        return getSubsequence(startIndex, _sentence.size());
    }

    public PenSentence getSubsequence(int startIndex, int finshIndex)
    {
        PenSentence seq = new PenSentence();

        if (startIndex >= 0)
        {
            for (int i = startIndex; i < _sentence.size() && i < finshIndex; i++)
            {
                seq._sentence.add(_sentence.get(i));
            }
        }
        return seq;
    }

    public String toString()
    {
        String str = "";
        for (int i = 0; i < _sentence.size(); i++)
        {
            str += _sentence.get(i);
            str += i == _sentence.size() - 1 ? "" : " ";
        }
        return str;
    }

    public static void main(String[] args)
    {
        PenSentence s = new PenSentence("Eu fui, ontem, \"comprar\" um guarda-chuva e 'e' dei 30€ e o meu mail: nseco@dei.uc.pt.");    
        System.out.println(s.toString());
    }

}
