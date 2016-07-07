package ontopt.pen;

import java.util.ArrayList;
import java.util.Arrays;

import opennlpcomum.items.PosTaggerWord;
import opennlpcomum.tokenizer.PosTaggerSimpleTokenizer;

public class SimpleSentence extends Sentence{

	private ArrayList<String> _sentence;

    public SimpleSentence(String sentence)
    {
        tokenize(sentence);
    }

    public void setSentence(String sentence)
    {
        tokenize(sentence);
    }

    /*protected void tokenize(String sentence)
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
    }*/

    protected void tokenize(String sentence)
    {
    	PosTaggerSimpleTokenizer simpleTokenizer = new PosTaggerSimpleTokenizer();
    	String[] tokenized = simpleTokenizer.tokenize(sentence);
    	_sentence = new ArrayList<String>(Arrays.asList(tokenized));
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
    
	public static String asString(ArrayList<String> list)
	{
		StringBuilder builder = new StringBuilder();
		
		for (int i = 0; i < list.size(); i++)
		{
			builder.append(list.get(i) + (i == list.size() - 1 ? "" : " "));
		}
		
		return builder.toString();
	}
    
    public String toString()
    {
		return asString(_sentence);
    }

    public static void main(String[] args)
    {
        SimpleSentence s = new SimpleSentence("Eu fui, ontem, \"comprar\" um guarda-chuva e 'e' dei 30€ e o meu mail: nseco@dei.uc.pt.");    
        System.out.println(s.toString());
    }
}
