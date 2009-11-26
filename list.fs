\ $ Id$

\ Doubly-linked lists in data memory. These are typically used to allocate
\ lists within the bodies of words. They should really be re-factored to
\ allow the use of dynamic memory.

\ Create a new list element
: LIST-ELEMENT \ ( n -- elem )
    HERE SWAP
    0 ,         \ back pointer
    ,           \ content
    0 , ;       \ forward pointer

\ Navigate an element
: <ELEMENT \ ( elem -- prev )
    @ ;
: <ELEMENT! \ ( nprev elem -- )
    ! ;
: ELEMENT> \ ( elem -- next )
    2 CELLS + @ ;
: ELEMENT>! \ ( nnext elem -- )
    2 CELLS + ! ;
: ELEMENT@ \ ( elem -- n )
    1 CELLS + @ ;
: ELEMENT! \ ( n elem -- )
    1 CELLS + ! ;

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

\ Map a word over all the elements of a list. The word applied
\ is expected to have a stack effect ( nbefore -- nafter ) and will
\ be passed each element value 
: MAP \ ( xt elem -- )
    SWAP >R
    BEGIN
	?DUP 0<>
    WHILE
	    DUP DUP ELEMENT@ R@ EXECUTE SWAP ELEMENT!
	    ELEMENT>
    REPEAT
    RDROP ;

\ Fold a word across the elements of a list "from the right". The word folded is
\ expected to have stack effect ( accbefore n -- accafter ) and is passed
\ an accululator and an element of the list from right to left
: FOLDR \ ( xt acc elem -- facc )
    -ROT >R
    BEGIN
	?DUP 0<>
    WHILE
	    TUCK ELEMENT@ R@ EXECUTE
	    SWAP <ELEMENT
    REPEAT
    RDROP ;

\ Fold a word across the elements of a list "from the left". The word folded is
\ expected to have stack effect ( accbefore n -- accafter ) and is passed
\ an accululator and an element of the list from left to right
: FOLDL \ ( xt acc elem -- facc )
    -ROT >R
    BEGIN
	?DUP 0<>
    WHILE
	    TUCK ELEMENT@ R@ EXECUTE
	    SWAP ELEMENT>
    REPEAT
    RDROP ;



    
