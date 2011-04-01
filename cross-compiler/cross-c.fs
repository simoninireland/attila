\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
\ All rights reserved.
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

\ The C-language cross-compiler
\
\ C primitives are stored in Attila source files alongside Attila code.
\ When loaded into the cross-compiler they are converted into
\ stylised C functions and output to a file ready for later compilation.


\ ---------- Some handy helper words ----------

\ Test whether two strings are case-insensitively equal, leaving
\ a flag and the first string
: S=CI? \ ( addr1 n1 addr2 n2 -- addr1 n1 f )
    2OVER S=CI ;

\ Test whether a list of strings contains the given string, working
\ *backwards* from the given node
: STRING-LIST-CONTAINS-BEFORE \ ( addr n elem -- f )
    BEGIN
	DUP 0<>
    WHILE
	    DUP 2OVER -ROT     \ addr n elem addr n elem
	    STRING-ELEMENT@ S=
	    2 PICK 0= OR IF
		DROP TRUE LEAVE
	    ELSE
		<ELEMENT
	    THEN
    REPEAT
    ROT 2DROP ;

\ Append a newline to a zstring
: Z+NL \ ( zaddr -- )
    ZCOUNT +
    NL OVER C!
    1+ NULL SWAP C! ;

\ Accumulate text up until the line beginning with the
\ given terminator into the given zstring
\ sd: should perhaps be <<Z, by analogy with Perl's naming convention?
: >>Z ( addr n zs -- zs )
    DUP NULLSTRING -ROT S>Z    \ zero the string
    BEGIN
	DUP
	REFILL IF
	    SOURCE 5 PICK 5 PICK S=CI? IF
		    \ terminator, exit
		    2DROP DROP
		    EMPTY-TIB
		    FALSE
	    ELSE
		\ non-terminator, continue
		TRUE
	    THEN
	ELSE
	    S" Input ended before terminator" ABORT
	THEN
    WHILE
	    \ accumulate the line into the zstring
	    Z+S DUP Z+NL
    REPEAT
    ROT 2DROP ;


\ ---------- Primitive and block name generator ----------

VARIABLE (#PRIM) 0 (#PRIM) !    \ uniques
DATA (PRIMNAME) 32 ALLOT        \ buffer for name

\ Create a new name
: (GENPRIMNAME) \ ( stemaddr stemn -- addr n )
    (PRIMNAME) SMOVE
    (PRIMNAME) COUNT (#PRIM) @ <# #S #> S+
    1 (#PRIM) +! 
    (PRIMNAME) COUNT ;

\ Create a new primitive name
: PRIMNAME \ ( -- addr n )
    S" prim" (GENPRIMNAME) ;

\ Create a new cblock name
: CBLOCKNAME \ ( -- addr n )
    S" block" (GENPRIMNAME) ;


\ ---------- Primitives and blocks ----------

\ List of primitives
0 A-ELEMENT VALUE PRIMITIVES

\ List of literal blocks
0 A-ELEMENT VALUE CBLOCKS


\ ---------- Primitive parser ----------

\ Buffer for the text (allows 5kb of source per primitive or literal block)
5 1024 * VALUE PRIMTEXT-SIZE
DATA (PRIMTEXT) PRIMTEXT-SIZE ALLOT

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS

\ Parse a C primitive, creating a locator word that holds all its details
: C:
    PARSE-WORD
    ALIGNED HERE ROT S,                    \ primitive's Forth-level name: ( naddr )

    PARSE-WORD S" (" S=CI? IF              \ compile primitive's primitive name
	2DROP PRIMNAME                     \ create a primitive name
    ELSE
	PARSE-WORD 2DROP                   \ consume the open bracket
    THEN

    ALIGNED HERE ROT S,                    \ store primitive's C-level name: ( naddr paddr )

    NULLSTRING STRING-ELEMENT              \ compile the parameters list
    BEGIN
	PARSE-WORD S" --" S=CI? NOT
    WHILE
	    STRING-ELEMENT OVER ELEMENT+>>
    REPEAT
    2DROP                                  \ argument list: ( naddr paddr al )

    NULLSTRING STRING-ELEMENT              \ compile the results list
    BEGIN
	PARSE-WORD S" )" S=CI? NOT
    WHILE
	    STRING-ELEMENT OVER ELEMENT+>>
    REPEAT
    2DROP                                  \ result list: ( naddr paddr al rl )

    S" ;C" (PRIMTEXT) >>Z                  \ compile text
    ALIGNED HERE SWAP Z,                   \ primitive text: ( naddr paddr al nl zaddr )

    CREATE-PRIMITIVE-LOCATOR               \ build the locator

    LASTXT EXECUTE >R
    R@ PRIMITIVE-NAME @ COUNT              \ cross-compile word onto target
    R@ PRIMITIVE-CFA @
    [CROSS] (HEADER,)

    R@ TARGET-XT !                        \ store txt of word in locator
    \ ." Primitive: " R@ PRIMITIVE-NAME @ COUNT TYPE CR
    R> A-ELEMENT PRIMITIVES ELEMENT+>> ;  \ add locator to chain of primitives

WORDLISTS>


\ ---------- C block parser ----------

\ Create a new C block
: (CBLOCK) \ ( zaddr addr n -- )
    (DATA)
    Z, ;

\ Retrieve the block text
: CBLOCK-TEXT \ ( loc -- addr n )
    ZCOUNT ;

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS

\ Parse a C header, creating a block
: CHEADER: \ ( -- )
    S" ;CHEADER" (PRIMTEXT) >>Z                 \ compile text
    CBLOCKNAME (CBLOCK)

    LASTXT EXECUTE A-ELEMENT CBLOCKS ELEMENT+>> ;

WORDLISTS>


\ ---------- Primitive and block generation ----------

\ Generate the declarations for arguments and results
: GENERATE-PRIMITIVE-VARIABLES \ ( loc -- )
    DUP PRIMITIVE-ARGUMENTS @ ELEMENT>           \ first element is always blank
    BEGIN                                        \ arguments
	?DUP 0<>
    WHILE
	    DUP STRING-ELEMENT@
	    2 PICK <ELEMENT STRING-LIST-CONTAINS-BEFORE NOT IF
		." CELL "
		DUP STRING-ELEMENT@ TYPE
		." ;" CR
	    THEN
	    ELEMENT>
    REPEAT

    DUP PRIMITIVE-ARGUMENTS @ DUP 0<> IF          \ results
	ELEMENT>>
    THEN
    SWAP PRIMITIVE-RESULTS @ ELEMENT>             \ first element is always blank
    BEGIN
	?DUP 0<>
    WHILE
	    DUP STRING-ELEMENT@                                        \ args res addr n
	    2DUP 5 PICK STRING-LIST-CONTAINS-BEFORE NOT IF             \ args res addr n 
		2 PICK <ELEMENT STRING-LIST-CONTAINS-BEFORE NOT IF     \ args res
		    ." CELL "
		    DUP STRING-ELEMENT@ TYPE
		    ." ;" CR
		THEN
	    ELSE
		2DROP
	    THEN
	    ELEMENT>
    REPEAT
    DROP ;

\ Generate the stack pops for the arguments
: GENERATE-PRIMITIVE-ARGUMENT-POPS \ ( loc -- )
    PRIMITIVE-ARGUMENTS @ ?DUP 0<> IF
	ELEMENT>>
	BEGIN
	    ?DUP 0<>
	WHILE
		DUP STRING-ELEMENT@ ?DUP 0> IF
		    TYPE
		    SPACE ." = POP_CELL();" CR
		ELSE
		    DROP
		THEN
		<ELEMENT
	REPEAT
    THEN ;

\ Generate the stack pushes for the results
: GENERATE-PRIMITIVE-RESULT-PUSHES \ ( loc -- )
    PRIMITIVE-RESULTS @ ELEMENT>    \ first element is always blank
    BEGIN
	?DUP 0<>
    WHILE
	    DUP STRING-ELEMENT@ ?DUP 0> IF
		." PUSH_CELL("
		TYPE
		." );" CR
	    ELSE
		DROP
	    THEN
	    ELEMENT>
    REPEAT ;

\ Generate the C source code for a primitive
: GENERATE-PRIMITIVE \ ( loc -- )
    DUP ." // " PRIMITIVE-NAME @ COUNT TYPE CR
    ." VOID" CR
    DUP PRIMITIVE-PRIMNAME @ COUNT TYPE ." ( XT _xt ) {" CR
    DUP GENERATE-PRIMITIVE-VARIABLES
    DUP GENERATE-PRIMITIVE-ARGUMENT-POPS
    DUP PRIMITIVE-TEXT @ ZCOUNT TYPE CR
    DUP GENERATE-PRIMITIVE-RESULT-PUSHES
    ." }" CR CR
    DROP ;

\ Generate a C declaration for a primitive
: GENERATE-PRIMITIVE-DECLARATION \ ( loc -- )
    ." VOID " PRIMITIVE-PRIMNAME @ COUNT TYPE ." ( XT );" CR ;

\ Generate declarations for all the primitives
: (GENERATE-PRIMITIVE-DECLARATION-FROM-ELEMENT) \ ( loc -- )
    A@ GENERATE-PRIMITIVE-DECLARATION ;
: GENERATE-PRIMITIVE-DECLARATIONS \ ( -- )
    ['] (GENERATE-PRIMITIVE-DECLARATION-FROM-ELEMENT) PRIMITIVES ELEMENT> MAP ;

\ Generate all the primitives
: (GENERATE-PRIMITIVE-FROM-ELEMENT) \ ( loc -- )
    A@ GENERATE-PRIMITIVE ;
: GENERATE-PRIMITIVES \ ( -- )
    ['] (GENERATE-PRIMITIVE-FROM-ELEMENT) PRIMITIVES ELEMENT> MAP ;

\ Generate all C blocks
: (GENERATE-CBLOCK-FROM-ELEMENT) \ ( loc -- )
    A@ CBLOCK-TEXT TYPE CR CR ;
: GENERATE-CBLOCKS \ ( -- )
    ['] (GENERATE-CBLOCK-FROM-ELEMENT) CBLOCKS ELEMENT> MAP ;
