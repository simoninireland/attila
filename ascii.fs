\ $Id$

\ Basic character operations, ASCII version 

\ ASCII characters are 1 byte long
1 CONSTANT /CHAR

\ Return the number of bytes needed to represent n chars
: CHARS ( n -- n ) ;

\ Character constants
32 CONSTANT BL
10 CONSTANT NL

\ Case testing
: UC? 65 ( A )  90 ( Z ) 1+ WITHIN ;
: LC? 97 ( a ) 122 ( z )  1+ WITHIN ;

\ Case conversion
: >UC \ ( c -- uc )
    DUP LC? IF
	97 ( a ) -  65 ( A ) +
    THEN ;
: >LC \ ( c -- lc )
    DUP UC? IF
	65 ( A )  - 97 ( a ) +
    THEN ;

\ Push the first character of a string onto the stack
: CHAR \ ( addr len -- c )
    DROP C@ ;

\ Test characters for equality. This is sometimes character-set dependent (although
\ we're probably just being overly picky)
: C= = ;
