PEN Parser 1.1
Hugo Gonçalo Oliveira 2009
===========================

Index:
1. Contents
2. Utilization
3. Writing Grammars
4. Examples
5. API
6. Additional documentation
7. License


1. Contents
-----------

This package contains the binaries and source code of PEN, a chart parser that implements the Earley algorithm.
It can be used to process sentences according to aby given context-free grammars.


2. Utilization
--------------

java -jar pen.jar <grammar_file> <setences_file>

* <grammar_file> is a file with a grammar, with the format described in the next item
* <sentences_file> is a file with a sentence per line
* Both file arguments are mandatory!

Example:
java -jar pen.jar grammar.txt sentences.txt


3. Writing Grammars
-------------------

The grammars are text files with rules described in the following format:

[comment...
WEIGHT # RULE_A ::= RULE_B <&> terminal <@> annotation


The following symbols are PEN's primitives:
	"[" the begining of a commented line
	"::=" the begining of a rule's body
	"<&>" conjunction between rules and terminal symbols
	"<?>" special symbol, that can be instantiated by any token
	"<>" an empty token
	"<@> an annotation
	">" includes the rules of another grammar

* Comments and annotations can be any kind of string
* WEIGHT must be an integer
* Rules are always written in uppercase characters or underscores ('_'), while terminal symbols should contain at least one lowercase character.
* PEN always starts a derivation from the rule "RAIZ", so this MUST ALWAYS be the first high level rule of a grammar.


4. Examples
-----------

Several examples can be found in directory 'examples' of this package.

Each example grammar has the name gramaticaX.txt and is made to process sentences contained in the files frasesX.txt.
For example, gramatica2.txt is made to process frases2.txt.


5. API
------
The API documentation can be found in directory 'doc' of this package.



6. Additional documentation (in Portuguese)
----------------------------------------

Presentation:
Available from http://eden.dei.uc.pt/~hroliv/phd/GoncaloOliveira2009_PEN.pdf

Using PEN to extract information from dictionary definitions:
http://linguateca.dei.uc.pt/papel/GoncaloOliveiraetal2008relPAPEL3.pdf


7. License
----------

This package is provided under the BSD license in the file LICENSE, included.
