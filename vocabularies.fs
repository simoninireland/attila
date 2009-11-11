\ $Id: vocabularies.fs,v 1.8 2007/05/23 15:41:48 sd Exp $

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

\ Vocabulary (word list) handling
\
\ Vocabularies allow the available words to be segmented in a fairly
\ weak fashion. A vocabulary order controls the search order for words
\ at compile-time, allowing one definition to mask another.
\
\ Vocabularies in most Forth-style languages are fairly ephemeral. In
\ Attila, however, a vocabulary is also the unit of (gross) memory
\ management, with each vocabulary having its own header, code and
\ data space. This allows for more flexible dynamic code management.
\ However, it measn that the Attila words are actually managing two
\ separate but related concepts: the Attila-level vocabulary (with
\ names, search order etc) and the vm-level vocabulary (for storage
\ management).
\
\ We represent vocabularies at the Attila level by the xt of the
\ word defining the vocabulary.
\
\ The ROOT vocabulary includes everything defined at the point that
\ vocabularies.fs is loaded, and is always the base vocabulary in the
\ search order. Since this will include the vocabulary words themselves,
\ we then can't forget the core of the system by accident.
\ Requires: createdoes.fs, variables.fs, strings.fs, loops.fs, counted-loops.fs

\ ---------- Vocabulary creation and access ----------

\ The maximum number of vocabularies available for searching -- define with care!
10 VALUE MAX-VOCABULARIES

\ The vocabulary search order
MAX-VOCABULARIES STACK VOCABULARIES

\ The current Attila-level vocabulary, which is (slightly) richer than
\ that used by the underlying virtual machinery
VARIABLE CURRENT-VOCABULARY

\ Check if we have vocabularies other than ROOT available
: (OTHER-VOCABULARIES?) \ ( -- f )
    VOCABULARIES #ST ;

\ Create a new vocabulary attachment point which adds its words to the
\ search order. The body of the word should be a pointer to the underlying
\ VM structure: for now we just generate a placeholder for it
: (VOCABULARY) \ ( "name" -- )
    CREATE
    0 ,                         \ placeholder for VM vocabulary structure
    LASTXT ,                    \ our xt, which represents us in the search order etc
    LASTHA ,                    \ our HA, so we can get our own name
                                \ sd: should we just compile it here as data?
  DOES> \ ( -- )
    (OTHER-VOCABULARIES?) IF
	VOCABULARIES ST-DROP    \ drop top vocab if it's not just root on the stack
    THEN
    CELL+ @ VOCABULARIES >ST ;  \ replace the top of the vocab stack with our xt

\ Create a new vocabulary
: VOCABULARY \ ( "name" -- )
    (VOCABULARY)
    (NEW-VOCABULARY) LASTXT >BODY ! ;

\ Return the underlying vocabulary structure
: VOC>(VOC) \ ( voc -- addr )
    >BODY @ ;

\ Return the name of a vocabulary
: VOC>NAME \ ( voc -- addr n )
    >BODY 2 CELLS + @ HA>NAME ;

\ Print the name of a vocabulary
: .VOC \ ( voc -- )
    VOC>NAME TYPE SPACE ;


\ ---------- Vocabulary management ----------

\ Put a "standard" vocabulary wrapper around the underlying root vocabulary
(VOCABULARY) ROOT (ROOT-VOCABULARY) LASTXT >BODY !

\ Duplicate the top of the search order
: ALSO \ ( -- )
    (OTHER-VOCABULARIES?) IF
	VOCABULARIES ST@
	VOCABULARIES >ST
    ELSE
	['] ROOT VOCABULARIES >ST
    THEN ;

\ Set the current VM vocabulary to that represented by the given Attila vocabulary
: SET-(CURRENT-VOCABULARY) \ ( voc -- )
    VOC>(VOC) (CURRENT-VOCABULARY) ! ;

\ Make the given vocabulary current, updating the Attila- and VM-level variables
: (DEFINITIONS) \ ( voc -- )
    DUP CURRENT-VOCABULARY !
    SET-(CURRENT-VOCABULARY) ;

\ Make the top-most vocabulary current for definitions, dropping
\ it from the search order in the process. This also updates the
\ virtual machine's view of the current directory
: DEFINITIONS \ (  -- )
    (OTHER-VOCABULARIES?) IF
	VOCABULARIES ST>
    ELSE
	['] ROOT
    THEN
    (DEFINITIONS) ;

\ Remove all vocabularies from the search order except root
: ONLY \ ( -- )
    VOCABULARIES #ST VOCABULARIES ST-NDROP ;

\ Drop the top-most vocabulary from the search order. This does not affect the
\ current vocabulary
: PREVIOUS
    (OTHER-VOCABULARIES?) IF
	VOCABULARIES ST-DROP
	THEN ;

\ Retrieve the order onto the stack topped with the number of
\ vocabularies pushed
: VOCABULARIES> \ ( -- vn-1 ...v0 n )
    ['] ROOT
    VOCABULARIES ST-ALL> 1+ ;

\ Print the current stack order
: ORDER \ ( -- )
    VOCABULARIES> 0 DO
	.VOC
    LOOP ;

\ Return the address of the vocabulary receiving definitions
: CURRENT \ ( -- voc )
    CURRENT-VOCABULARY @ ;

\ Create a marker that, when executed, will forget all definitions in the
\ current vocabulary after and including the marker without affecting any
\ words in other vocabularies
: (MARKER) \ ( addr n -- )
    >R >R
    CURRENT VOC>(VOC) VOCABULARY-STATE@
    R> R> (CREATE)
    CURRENT VOC>(VOC) ,
    DUP 1+ 0 DO , LOOP
  DOES>
    DUP @ >R
    CELL+ DUP @ 0 DO
	DUP DUP @ I - CELLS + @ SWAP
    LOOP DROP R> VOCABULARY-STATE! ;

\ Create a marker from the next word in the input
: MARKER \ ( "name" -- )
    PARSE-WORD (MARKER) ;


\ ---------- Finding and listing ----------
\ sd: these are slightly complicated by the fact that LASTHA etc always refer
\ to the last ha of the *current* vocabulary, so we need to make a vocabulary
\ current to get at its words. The upside is that it puts the complexity at
\ Attila level rather than in the virtual machine

\ Messy way to save and restore the current VM vocabulary temporarily within search
\ words -- but there's no practical need to do anything cleaner
VARIABLE (SAVED-CURRENT-VOCABULARY)
: SAVE-(CURRENT-VOCABULARY)    (CURRENT-VOCABULARY) @ (SAVED-CURRENT-VOCABULARY) ! ;
: RESTORE-(CURRENT-VOCABULARY) (SAVED-CURRENT-VOCABULARY) @ (CURRENT-VOCABULARY) ! ;

\ Find the top-most definition of a word in the search order -- messy, messy...
VARIABLE (WORD-TO-FIND) 0 ,
: (TO-FIND) (WORD-TO-FIND) DUP @ SWAP CELL+ @ ;
: (FIND-IN-VOCABULARIES) \ ( addr len -- xt | 0 )
    SAVE-(CURRENT-VOCABULARY)
    SWAP (WORD-TO-FIND) SWAP               \ len wtf addr
    OVER ! CELL+ !
    VOCABULARIES> 0 DO
	(TO-FIND) -ROT SET-(CURRENT-VOCABULARY)
	LASTHA (FIND) ?DUP IF
	    \ we've found it, so drop the other vocabularies and return
	    VOCABULARIES #ST I - SWAP >R  \ I depends on return stack depth
	    ?DUP IF
		0 DO DROP LOOP
	    THEN
	    R> RESTORE-(CURRENT-VOCABULARY) EXIT
	THEN LOOP
    \ if we get here, we failed to find the word
    0 RESTORE-(CURRENT-VOCABULARY) ;

\ Return the last ha defined in the given vocabulary
: VOC>LASTHA \ ( voc -- ha )
    SAVE-(CURRENT-VOCABULARY) SET-(CURRENT-VOCABULARY)
    LASTHA
    RESTORE-(CURRENT-VOCABULARY) ;
    
\ List all the available words in a given vocabulary
: (.WORDS) \ ( voc -- )
    VOC>LASTHA BEGIN
	?DUP
    WHILE
	    DUP HA>NAME TYPE SPACE
	    HA>LFA @
    REPEAT ;

\ List all the available words in the search order, ignoring
\ adjacent duplicate vocabularies
: WORDS \ ( -- )
    (OTHER-VOCABULARIES?) IF
	VOCABULARIES> 1- 0 DO
	    2DUP <> IF
		(.WORDS)
	    THEN
	LOOP
    ELSE
	['] ROOT
    THEN
    (.WORDS) ;
	    

\ ---------- Set-up ----------

\ Initialise the search order
ONLY ALSO DEFINITIONS

\ Install the vocabulary-aware FIND as the system-wide find policy
' (FIND-IN-VOCABULARIES) (FIND-POLICY) !
