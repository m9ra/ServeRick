package ontopt.pen;

import java.util.ArrayList;

public abstract class Sentence {

	protected ArrayList<String> _sentence;
	
	public Sentence()
	{
		
	}
	
    public void setSentence(String sentence)
    {
        tokenize(sentence);
    }
	
	protected abstract void tokenize(String sentence);
	
	 /**
     * Returns the word at the specified index
     * 
     * @param index
     *            the index to look up
     * @return The word at the specified index
     */

    public abstract String getWord(int index);
    
    public abstract int getSentenceSize();
   
    public abstract String toString();
}
