\ $Id$

\ This file is part of Attila, a minimal threaded interpretive language
\ Copyright (c) 2007, UCD Dublin. All rights reserved.
\
\ Attila is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.
\
\ Attila is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ Separated wordlists.
\
\ Wordlists allow the available words to be segmented in a fairly
\ weak fashion. A word list order controls the search order for words
\ at compile-time, allowing one definition to mask another.
\
\ The ROOT-WORDLIST contains the wordlist words and  is always the base
\ vocabulary in the search order. Since this will include the wordlist
\ management words themselves, we can't forget the core of the system by
\ accident. The FORTH-WORDLIST contains the rest of the words defined
\ up until this file is included, and is made current so it gets all subsequent
\ words (unless you choose to change the current wordlist). This
\ separation means it's possible to not search the main system (by not having
\ FORSTH-WORDLIST in the search order) while still being able to get it back
\ (by using words from the ROOT-WORDLIST, which is always accessible).
\
\ A vocabulary is a word that establishes a wordlist on the top of the
\ search order.
\ Requires: createdoes.fs, variables.fs, strings.fs, loops.fs, counted-loops.fs

\ Grab the word on top of the system for use later
LASTXT

\ ---------- Word list creation and access ----------

\ The maximum number of word lists available for searching -- define with care!
\ There can be as many wordlists as you like, but only this many can be
\ in the search order
10 VALUE MAX-WORDLISTS

\ Grab this word as the bootom of the root wordlist
LASTXT

\ The wordlist search order
MAX-WORDLISTS STACK WORDLISTS

\ The current wordlist that gets new definitions
VARIABLE CURRENT-WORDLIST

\ Check if we have wordlists other than ROOT available
: (OTHER-WORDLISTS?) \ ( -- f )
    WORDLISTS #ST 0> ;

\ Create a new, empty, anonymous wordlist. We use the word list's body
\ pointer as its identifier. The body stores the xt of the last
\ word added to this word list
: WORDLIST \ ( -- wid )
    HERE
    0 , ;     \ last xt defined in this word list

\ Retrieve the xt of the top-most word on a wordlist, or 0 if
\ the word list is empty
: WID> \ ( wid -- xt )
    @ ;

\ Update a wordlist with a new top word 
: >WID \ ( xt wid -- )
    ! ;


\ ---------- Word list management ----------

\ A reference to the root wordlist (holding the wordlist words) and the
\ main (Forth) wordlist holding the rest of the system 
WORDLIST CONSTANT ROOT-WORDLIST
WORDLIST CONSTANT FORTH-WORDLIST

\ Return the wordlist receiving definitions
: GET-CURRENT \ ( -- wid )
    CURRENT @ ;

\ Set the wordlist receiving definitions, updating LAST to reflect the last word
\ defined in this wordlist
: SET-CURRENT \ ( wid -- )
    DUP CURRENT !
    WID> LAST ! ;

\ Retrieve the top-most wordlist in the search order without affecting it
: CONTEXT
    (OTHER-WORDLISTS?) IF
	WORDLISTS ST@
    ELSE
	ROOT-WORDLIST
    THEN ;

\ Drop the top-most wordlist from the search order. This does not affect the
\ current wordlist: it is possible (though unusual) to compile into a wordlist
\ we don't search
: PREVIOUS
    (OTHER-WORDLISTS?) IF
	WORDLISTS ST-DROP
    THEN ;

\ Duplicate the top of the search order
: ALSO \ ( -- )
    CONTEXT WORDLISTS >ST ;

\ Make the top-most wordlist current for definitions, dropping
\ it from the search order in the process
: DEFINITIONS \ (  -- )
    CONTEXT SET-CURRENT
    PREVIOUS ;

\ Remove all wordlists from the search order except root
: ONLY \ ( -- )
    WORDLISTS #ST WORDLISTS ST-NDROP ;

\ Retrieve the order onto the stack topped with the number of
\ wordlists pushed. The root wordlists is always at the bottom of
\ the order
: GET-ORDER \ ( -- widn-1 ...wid0 n )
    ROOT-WORDLIST
    WORDLISTS ST-ALL> 1+ ;

\ Replace the top wordlist in the search order with the given one
: >ORDER ( wid -- )
    PREVIOUS
    WORDLISTS >ST ;


\ ---------- Finding ----------

\ Find the definition of a word in a wordlist
\ sd: slightly non-standard as we don't use counted addresses
: SEARCH-WORDLIST \ ( addr n wid -- 0 | xt 1 | xt -1 )
    WID> ?DUP IF
	(FIND)
    ELSE             \ empty wordlist
	2DROP 0
    THEN ;

\ Find the definition of a word in the current search order -- messy, messy...
\ (and un-optimised for repeated word lists)
DATA WORD-TO-FIND 2 CELLS ALLOT
: (FIND-IN-WORDLISTS) \ ( addr n -- 0 | xt 1 | xt -1 )
    WORD-TO-FIND 2!
    GET-ORDER
    BEGIN
	DUP 0>
    WHILE
	    1- SWAP
	    WORD-TO-FIND 2@ -ROT SEARCH-WORDLIST
	    ?DUP IF
		>R >R
		?DUP 0> IF
		    0 DO DROP LOOP
		THEN
		R> R>
		LEAVE
	    THEN
    REPEAT
;
    

\ ---------- Word listing ----------

\ Print the name of a word followed by a space
: .WORD \ ( xt -- )
    >NAME TYPE SPACE ;

\ List all the words in a wordlist
: .WORDLIST ( wid -- )
    WID> ?DUP IF
	BEGIN
	    DUP .WORD
	    >LFA @
	    ?DUP 0=
	UNTIL
    THEN ;

\ List all the words in the current search order, ignoring adjacent
\ duplicate wordlists (but *not* ignoring non-adjacent duplicates)
: WORDS \ ( -- )
    0 GET-ORDER DROP BEGIN
	?DUP 0<>
    WHILE
	2DUP = NOT IF
	    .WORDLIST
	ELSE
	    DROP
	THEN
    REPEAT ;
    
\ ---------- Vocabularies, words that select word lists ----------

\ Create a vocabulary that will add itself to the search order when executed
: (VOCABULARY) \ ( wid addr n -- )
    (CREATE)
    ,                \ the wid is in our body address
  DOES> \ ( addr -- )
    @ >ORDER ;

\ Create a new vocabulary
: VOCABULARY \ ( "name" -- )
    WORDLIST
    PARSE-WORD (VOCABULARY) ;

\ Vocabulary words for the root and Forth wordlists
ROOT-WORDLIST PARSE-WORD ROOT (VOCABULARY)
FORTH-WORDLIST PARSE-WORD FORTH (VOCABULARY)


\ ---------- Set-up ----------

\ Update current wordlist with each new word defined by chaining
\ the appropriate behaviour onto the front of the (END-DEFINITION) hook
: (END-DEFINITION-WORDLISTS) \ ( xt -- xt )
    DUP GET-CURRENT >WID
    [ (END-DEFINITION) @ ] LITERAL EXECUTE ;
' (END-DEFINITION-WORDLISTS) (END-DEFINITION) !

\ Patch the Forth wordlist to include everything defined up to this point,
\ which includes the root wordlist
LASTXT FORTH-WORDLIST >WID

\ FORTH-WORDLIST is searched and current
ONLY FORTH ALSO DEFINITIONS

\ Update the global FIND behaviour to be wordlist-enabled
' (FIND-IN-WORDLISTS) (FIND-BEHAVIOUR) !

\ Finally we separate the root and Forth wordlists by patching the top
\ of the root wordlist to be the last word defined, breaking the word
\ chain at the first word in this file, and patching the top of the
\ Forth wordlist to be the first word before this file. Phew!
LASTXT ROOT-WORDLIST >WID
0 SWAP >LFA !
FORTH-WORDLIST >WID
ONLY FORTH ALSO DEFINITIONS
