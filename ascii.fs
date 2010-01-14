\ $Id$

\ Character operations, ASCII version 

\ ASCII characters are 1 byte long
1 CONSTANT /CHAR

\ Return the number of bytes needed to represent n chars
: CHARS ( n -- n ) ;

\ Character constants
32 CONSTANT BL
10 CONSTANT NL

\ Push the first character of a string onto the stack
: CHAR \ ( addr len -- c )
    DROP C@ ;

\ Extract the first character of the next word in the input stream
: [CHAR] \ ( "word" -- )
    PARSE-WORD CHAR
    POSTPONE LITERAL ; IMMEDIATE

\ Convert character to upper case
: C>UC \ ( c -- uc )
    DUP [CHAR] a [CHAR] z 1+ WITHIN IF
	[CHAR] a - [CHAR] A +
    THEN ;

\ Convert character to lower case
: C>LC \ ( c -- lc )
    DUP [CHAR] A [CHAR] Z 1+ WITHIN IF
	[CHAR] A - [CHAR] a +
    THEN ;
    
\ Test characters for equality
: C= = ;

\ Test characters for case-insensitive equality
: C=CI \ ( c1 c2 -- f )
    C>UC SWAP C>UC C= ;
