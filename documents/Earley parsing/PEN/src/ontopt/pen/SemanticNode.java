package ontopt.pen;

import java.io.Externalizable;
import java.io.IOException;
import java.io.ObjectInput;
import java.io.ObjectOutput;
import java.util.Enumeration;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.Vector;

//import javax.swing.tree.TreeNode;

/**
 * <p>
 * Class from PEN Parser.
 * </p>
 * <p>
 * This class represents a node in the derivation tree.
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

public class SemanticNode implements Externalizable, Comparable
{

    protected String annotation;

    protected int weight;

    /**
     * The parent of this node
     */
    protected SemanticNode parent;

    /**
     * The children of this node
     */
    protected LinkedList<SemanticNode> children;

    /**
     * The label associated to this node
     */
    protected String label;

    /**
     * The constructor.
     */
    public SemanticNode()
    {
        children = new LinkedList<SemanticNode>();
    }

    /**
     * The constructor
     * 
     * @param label
     *            Default label to use
     */
    public SemanticNode(String label, int weight, String annotation)
    {
        this();
        this.label = label;
        this.weight = weight;
        this.annotation = annotation;
    }

    /**
     * Creates a copy of this node without the parents. Used for Serialization across a network
     * 
     * @return A clone of this node
     */
    public SemanticNode getCloneOfNode()
    {
        return getCloneOfNode(this);
    }

    /**
     * Creates a copy of the node without the parents. Used for Serialization across a network
     * 
     * @param original
     *            the node to clone
     * @return A clone of the node
     */
    private SemanticNode getCloneOfNode(SemanticNode original)
    {
        SemanticNode clone = new SemanticNode(original.label, original.weight, original.annotation);
        Iterator<SemanticNode> children = original.getChildren().iterator();

        while (children.hasNext())
        {
            clone.addChild(getCloneOfNode((SemanticNode) children.next()));
        }
        return clone;
    }

    /**
     * Sets the label of this node
     * 
     * @param label
     *            The label of the node
     */
    public void setLabel(String label)
    {
        this.label = label;
    }

    /**
     * Sets the parent of this node
     * 
     * @param parent
     */
    public void setParent(SemanticNode parent)
    {
        this.parent = parent;
    }

    /**
     * Sets the children of this node
     * 
     * @param children
     *            the list of children
     */
    public void setChildren(LinkedList<SemanticNode> children)
    {
        this.children = children;
    }

    /**
     * Gets the left most node
     * 
     * @return the left most node
     */
    public SemanticNode getLeftCorner()
    {
        return getLeftCorner(this);
    }

    /**
     * Gets the left most node
     * 
     * @param node
     *            The from where to start the search
     * @return The left most node
     */
    private SemanticNode getLeftCorner(SemanticNode node)
    {
        SemanticNode leftChild = (SemanticNode) node.getChildren().get(0);

        if (leftChild.getChildren().size() == 0)
        {
            return leftChild;
        }
        else
        {
            return getLeftCorner(leftChild);
        }
    }

    /**
     * Adds a child to this node
     * 
     * @param child
     *            The child to add
     */
    public void addChild(SemanticNode child)
    {
        child.parent = this;
        children.add(child);
    }

    /**
     * Strips the leaves off the tree, if they are words
     * 
     * @return A string representing the sentence
     */
    public String stripSentence()
    {
        String sentence = "";

        for (Iterator<SemanticNode> i = getTerminals(this).iterator(); i.hasNext();)
        {
            sentence += i.next().toString() + " ";
        }

        return sentence.trim();
    }

    /**
     * Returns a list of leaves
     * 
     * @return a list of leaves
     */
    public LinkedList<SemanticNode> getTerminals()
    {
        return getTerminals(this);
    }

    /**
     * Returns a list of leaves
     * 
     * @param node
     *            The node from which to get the leaves from.
     * @return A list of leaves
     */
    private LinkedList<SemanticNode> getTerminals(SemanticNode node)
    {
        LinkedList<SemanticNode> terminals = new LinkedList<SemanticNode>();
        SemanticNode n;
        Iterator<SemanticNode> nodeChildren = node.getChildren().iterator();

        while (nodeChildren.hasNext())
        {
            n = (SemanticNode) nodeChildren.next();
            if (n.isLeaf())
            {
                terminals.add(n);
            }
            else
            {
                terminals.addAll(getTerminals(n));
            }
        }

        return terminals;
    }

    /**
     * Returns a list of nodes with a specified label
     * 
     * @param POSLabel
     *            The label to search for
     * @return A list of nodes with the specified label
     */
    public LinkedList<SemanticNode> getNodes(String POSLabel)
    {
        return getNodes(this, POSLabel);
    }

    /**
     * 
     * @param node
     *            The node to from where to start the search
     * @param POSLabel
     *            the label to search for
     * @return A list nodes with the specified label.
     */
    private LinkedList<SemanticNode> getNodes(SemanticNode node, String POSLabel)
    {
        LinkedList<SemanticNode> pos = new LinkedList<SemanticNode>();
        Iterator<SemanticNode> nodeChildren = node.getChildren().iterator();
        String label;

        label = node.label;

        if (label.equals(POSLabel) || POSLabel.equals(""))
        {
            pos.add(node);
        }

        while (nodeChildren.hasNext())
        {
            pos.addAll(getNodes((SemanticNode) nodeChildren.next(), POSLabel));
        }

        return pos;
    }

    /**
     * Implementation of the equals method. Overrides the deafult implementation.
     * 
     * @param o
     *            The object to compare to.
     * @return True if they are equal, false otherwise
     */
    public boolean equals(Object o)
    {
        if (!(o instanceof SemanticNode))
        {
            return false;
        }

        SemanticNode n = (SemanticNode) o;

        if (this.label.equals(n.label))
        {
            return isEqual(this.getChildren(), n.getChildren());
        }

        return false;
    }

    /**
     * Checks if this node subsumes the parameter
     * 
     * @param anotherNode
     *            The node to compare to
     * @return Return true if this node subsumes anotherNode
     */
    protected boolean subsumes(SemanticNode anotherNode)
    {
        if (this.label.equals(anotherNode.label))
        {
            return isSubsumed(this.getChildren(), anotherNode.getChildren());
        }

        return false;
    }

    /**
     * The recursive method for checking subsumation. Checks if the subsumer list subsumes the subsumee list
     * 
     * @param subsumer
     *            The supposed subsumer list
     * @param subsumee
     *            The supposed subsumee list
     * @return True if the first list subsumes the second
     */
    private boolean isSubsumed(LinkedList<SemanticNode> subsumer, LinkedList<SemanticNode> subsumee)
    {
        if (subsumer.size() == 0 && subsumee.size() == 0)
        {
            return true;
        }

        if (subsumer.size() > subsumee.size())
        {
            return false;
        }

        SemanticNode nodeOfSubsumer;
        SemanticNode nodeOfSubsumee;
        LinkedList<SemanticNode> childrenOfSubsumer = new LinkedList<SemanticNode>();
        LinkedList<SemanticNode> childrenOfSubsumee = new LinkedList<SemanticNode>();
        int k = 0;
        boolean found = false;

        for (int i = 0; i < subsumer.size(); i++)
        {
            if (k >= subsumee.size())
            {
                return false;
            }

            found = false;
            nodeOfSubsumer = (SemanticNode) subsumer.get(i);
            nodeOfSubsumee = (SemanticNode) subsumee.get(k);

            if (!(nodeOfSubsumer.label.equals(nodeOfSubsumee.label) && nodeOfSubsumer.parent.label.equals(nodeOfSubsumee.parent.label)))
            {
                if (subsumer.size() == subsumee.size() || k == subsumee.size() - 1)
                {
                    return false;
                }
                else
                // check if the rest of the nodes exist at this level
                {
                    k++;
                    // childrenOfSubsumee.addAll(nodeOfSubsumee.getChildren());
                    for (; k < subsumee.size(); k++)
                    {
                        nodeOfSubsumee = (SemanticNode) subsumee.get(k);
                        if ((nodeOfSubsumer.label.equals(nodeOfSubsumee.label) && nodeOfSubsumer.parent.label.equals(nodeOfSubsumee.parent.label)))
                        {
                            found = true;
                            break;
                        }
                        // childrenOfSubsumee.addAll(nodeOfSubsumee.getChildren());
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
            }

            k++;

            childrenOfSubsumee.addAll(nodeOfSubsumee.getChildren());
            childrenOfSubsumer.addAll(nodeOfSubsumer.getChildren());
        }

        return isSubsumed(childrenOfSubsumer, childrenOfSubsumee);
    }

    /**
     * Checks if the two lists have the same nodes
     * 
     * @param tree1
     * @param tree2
     * @return
     */
    private boolean isEqual(LinkedList<SemanticNode> tree1, LinkedList<SemanticNode> tree2)
    {
        SemanticNode node1;
        SemanticNode node2;
        LinkedList<SemanticNode> children1 = new LinkedList<SemanticNode>();
        LinkedList<SemanticNode> children2 = new LinkedList<SemanticNode>();
        Iterator<SemanticNode> i1;
        Iterator<SemanticNode> i2;

        if (tree1.size() != tree2.size())
        {
            return false;
        }

        if (tree1.size() == 0)
        {
            return true;
        }

        i1 = tree1.iterator();
        i2 = tree2.iterator();
        while (i1.hasNext())
        {
            node1 = (SemanticNode) i1.next();
            node2 = (SemanticNode) i2.next();

            if (!(node1.label.equals(node2.label) && node1.parent.label.equals(node2.parent.label)))
            {
                return false;
            }
            else
            {
                children1.addAll(node1.getChildren());
                children2.addAll(node2.getChildren());
            }
        }

        return isEqual(children1, children2);
    }

    // Methods needed for TreeNode Interface
    public LinkedList<SemanticNode> getChildren()
    {
        return children;
    }

    /*public TreeNode getParent()
    {
        return parent;
    }*/

    /*public int getIndex(TreeNode node)
    {
        return children.indexOf(node);
    }*/

    public int getChildCount()
    {
        return children.size();
    }

    /*public TreeNode getChildAt(int index)
    {
        return (TreeNode) children.get(index);
    }*/

    public boolean isLeaf()
    {
        return (children == null || children.size() == 0);
    }

    public boolean getAllowsChildren()
    {
        return true;
    }

    public boolean isRoot()
    {
        return (parent == null);
    }

    public String toString()
    {
        return label;
    }

    public Enumeration<SemanticNode> children()
    {
        return (new Vector<SemanticNode>(children)).elements();
    }

    // Methods needed for Serialization
    public void writeExternal(ObjectOutput out) throws IOException
    {
        out.writeObject(parent);
        out.writeObject(children);
        out.writeObject(label);
    }

    public void readExternal(ObjectInput in) throws IOException, ClassNotFoundException
    {
        parent = (SemanticNode) in.readObject();
        children = (LinkedList<SemanticNode>) in.readObject();
        label = (String) in.readObject();
    }

    public int getWeight()
    {
        return weight;
    }

    public String getAnnotation()
    {
        return annotation;
    }

    public int getScore()
    {
        int score = this.weight;
        for (Enumeration<SemanticNode> en = children(); en.hasMoreElements();)
            score += en.nextElement().getScore();

        return score;
    }

    public int compareTo(Object anotherNode)
    {
        if (anotherNode instanceof SemanticNode)
        {
            return getScore() - ((SemanticNode) anotherNode).getScore();
        }

        return 0;
    }

    public String getLabel()
    {
        return label;
    }

}