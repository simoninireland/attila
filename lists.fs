\ $Id$

\ Doubly-linked lists in data memory. These are typically used to allocate
\ lists within the bodies of words.
\
\ This code should really be re-factored to allow the use of dynamic memory.
\
\ The element value is of arbitrary size, and nothing forces all elements
\ to be the same, as long as you can avoid overruns.

\ Create the control part of new list element
: (LIST-ELEMENT) \ ( -- elem )
    HERE
    0 ,         \ back pointer
    0 , ;        \ forward pointer

\ Navigate an element
: <ELEMENT \ ( elem -- prev )
    @ ;
: <ELEMENT! \ ( nprev elem -- )
    ! ;
: ELEMENT> \ ( elem -- next )
    1 CELLS + @ ;
: ELEMENT>! \ ( nnext elem -- )
    1 CELLS + ! ;
: ELEMENT-VALUE \ ( elem -- vaddr )
    2 CELLS + ;

\ Lists of cells
: CELL-ELEMENT \ ( n -- elem )
    (LIST-ELEMENT) SWAP , ;
: CELL-ELEMENT@ \ ( elem -- n )
    ELEMENT-VALUE @ ;
: CELL-ELEMENT! \ ( n elem -- )
    ELEMENT-VALUE ! ;

\ Lists of words (xts)
: XT-ELEMENT \ ( xt -- elem )
    (LIST-ELEMENT) SWAP , ;
: XT-ELEMENT@ \ ( elem -- xt )
    ELEMENT-VALUE XT@ ;
: XT-ELEMENT! \ ( xt elem -- )
    ELEMENT-VALUE XT! ;

\ Lists of addresses
: A-ELEMENT \ ( addr -- elem )
    (LIST-ELEMENT) SWAP , ;
: A-ELEMENT@ \ ( elem -- addr )
    ELEMENT-VALUE A@ ;
: A-ELEMENT! \ ( addr elem -- )
    ELEMENT-VALUE A! ;

\ Lists of (constant) strings
: STRING-ELEMENT \ ( addr n -- elem )
    (LIST-ELEMENT) ROT S, ;
: STRING-ELEMENT@ \ ( elem -- addr n )
    ELEMENT-VALUE COUNT ;

\ Traverse a list to the first or last element
: <<ELEMENT \ ( elem -- first )
    ?DUP 0<> IF
	BEGIN
	    DUP <ELEMENT
	    ?DUP 0<>
	WHILE
		NIP
	REPEAT
    THEN ;
: ELEMENT>> \ ( elem -- last )
    ?DUP 0<> IF
	BEGIN
	    DUP ELEMENT>
	    ?DUP 0<>
	WHILE
		NIP
	REPEAT
    THEN ;
	
\ Link elements, adding elem1 after (or before) elem2
: ELEMENT+ \ ( elem1 elem2 -- )
    SWAP >R
    DUP ELEMENT> R@ ELEMENT>!  \ elem1's next is elem2's next
    R@ OVER ELEMENT>!          \ elem1 is elem2's next
    R> <ELEMENT! ;             \ elem2 is elem1's previous
: +ELEMENT \ ( elem1 elem2 -- )
    SWAP >R
    DUP <ELEMENT R@ <ELEMENT!  \ elem1's next is elem2's next
    R@ OVER <ELEMENT!          \ elem1 is elem2's previous
    R> ELEMENT>! ;             \ elem2 is elem1's next

\ Appending and prepending from anywhere in a list
: ELEMENT+>> ELEMENT>> ELEMENT+ ;
: <<+ELEMENT <<ELEMENT +ELEMENT ;

\ Return the length of a list, counting forwards from the given element
: LIST-LENGTH \ ( elem -- n )
    0 SWAP
    BEGIN
	?DUP 0<>
    WHILE
	    >R 1+ R>
	    ELEMENT>
    REPEAT ;


\ ---------- "Higher-order" application words ----------
\ sd: Note that these words work on list elements, *not*
\ complete lists: if you pass an element halfway along a list
\ to MAP it will apply only from there to the end
\ sd: Note further that it's your problem to make these
\ works work polymorphically: the words applied need to know
\ what sort of value they're expecting

\ Map a word over all the elements of a list. The word applied
\ is expected to have a stack effect ( vaddr -- ) and will
\ be passed the address of each element's value in turn
: MAP \ ( xt elem -- )
    SWAP >R
    BEGIN
	?DUP 0<>
    WHILE
	    DUP ELEMENT-VALUE R@ EXECUTE
	    ELEMENT>
    REPEAT
    RDROP ;

\ Fold a word across the elements of a list "from the right". The word folded is
\ expected to have stack effect ( accbefore vaddr -- accafter ) and is passed
\ an accululator and an element's value address up the list from right to left
: FOLDR \ ( xt acc elem -- facc )
    -ROT >R
    BEGIN
	?DUP 0<>
    WHILE
	    TUCK ELEMENT-VALUE R@ EXECUTE
	    SWAP <ELEMENT
    REPEAT
    RDROP ;

\ Fold a word across the elements of a list "from the left". The word folded is
\ expected to have stack effect ( accbefore vaddr -- accafter ) and is passed
\ an accululator and an element's value address down the list from left to right
: FOLDL \ ( xt acc elem -- facc )
    -ROT >R
    BEGIN
	?DUP 0<>
    WHILE
	    TUCK ELEMENT-VALUE R@ EXECUTE
	    SWAP ELEMENT>
    REPEAT
    RDROP ;



    
