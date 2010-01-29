\ $Id$

\ The primitive cross-compiler
\
\ Primitives are stored in Attila source files alongside Attila code.
\ When loaded into the cross-compiler they are converted into
\ stylised C functions and output to a file ready for later compilation.
\
\ This code performs the same functions as tools/primgen does for
\ bootstrapping Attila.

\ ---------- Some handy helper words ----------

\ Test whether two strings are case-insensitively equal, leaving
\ a flag and the first string
: S=CI? \ ( addr1 n1 addr2 n2 -- add1 n1 f )
    2OVER S=CI ;

\ Test whether a list of strings contains the given string, working
\ *backwards* from the given node
: STRING-LIST-CONTAINS-BEFORE \ ( addr n elem -- f )
    BEGIN
	DUP 0<>
    WHILE
	    DUP 2OVER -ROT     \ addr n elem addr n elem
	    STRING-ELEMENT@ S= IF
		DROP TRUE LEAVE
	    ELSE
		<ELEMENT
	    THEN
    REPEAT
    ROT 2DROP ;


\ ---------- Primitive and block name generator ----------

VARIABLE (#PRIM) 0 (#PRIM) !    \ uniques
DATA (PRIMNAME) 32 ALLOT        \ buffer for name

\ Move a string
: SMOVE \ ( addr1 n addr2 -- )
    ROT 1+ ROT 1- ROT SWAP CMOVE ;

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

\ Buffer for the text (allows 20kb of source per primitive or literal block)
DATA (PRIMTEXT) 20 1024 * ALLOT

\ Append a newline to a zstring
: Z+NL \ ( zaddr -- )
    ZCOUNT +
    NL OVER C!
    1+ NULL SWAP C! ;

\ Return the last primitive
: LAST-PRIMITIVE LASTXT EXECUTE ;

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS

\ Parse a primitive, creating a locator word that holds all its details
: PRIMITIVE:
    PARSE-WORD 2DUP
    (PRIMITIVE-LOCATOR)                    \ compile a locator
    LAST-PRIMITIVE PRIMITIVE-NAME,         \ compile the primitive's name
    
    PARSE-WORD S" (" S=CI? IF              \ compile primitive's primitive name
	2DROP PRIMNAME                     \ create a primitive name
    ELSE
	PARSE-WORD 2DROP                   \ consume the open bracket
    THEN
    LAST-PRIMITIVE PRIMITIVE-PRIMNAME,

    LAST-PRIMITIVE PRIMITIVE-ARGUMENTS,    \ compile the parameters list
    BEGIN
	PARSE-WORD S" --" S=CI? NOT
    WHILE
	    STRING-ELEMENT DUP -ROT ELEMENT+
    REPEAT
    2DROP DROP

    LAST-PRIMITIVE PRIMITIVE-RESULTS,      \ compile the results list
    BEGIN
	PARSE-WORD S" )" S=CI? NOT
    WHILE
	    STRING-ELEMENT DUP -ROT ELEMENT+
    REPEAT
    2DROP DROP

    NULLSTRING (PRIMTEXT) S>Z              \ compile text
    (PRIMTEXT)
    BEGIN
	DUP
	REFILL IF
	    SOURCE 1- S" ;PRIMITIVE" S=CI? IF
		2DROP DROP
		REFILL DROP
		FALSE
	    ELSE
		TRUE
	    THEN
	ELSE
	    S" Input ended before ;PRIMITIVE" ABORT
	THEN
    WHILE
	    Z+S DUP Z+NL
    REPEAT
    LAST-PRIMITIVE PRIMITIVE-TEXT,

    LAST-PRIMITIVE PRIMITIVE-NAME
    LAST-PRIMITIVE PRIMITIVE-CFA
    [CROSS] (HEADER,)
    LAST-PRIMITIVE TARGET-XT,

    LAST-PRIMITIVE A-ELEMENT PRIMITIVES ELEMENT+>> ; \ chain primitives

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

\ Parse a C block, creating a block
: C: \ ( -- )
    NULL (PRIMTEXT) C!
    (PRIMTEXT)
    BEGIN
	DUP
	REFILL IF
	    SOURCE 1- S" ;C" S=CI? IF
		2DROP DROP
		REFILL DROP
		FALSE
	    ELSE
		TRUE
	    THEN
	ELSE
	    S" Input ended before ;C" ABORT
	THEN
    WHILE
	    Z+S DUP Z+NL
    REPEAT

    CBLOCKNAME (CBLOCK)
    LASTXT EXECUTE A-ELEMENT CBLOCKS ELEMENT+>> ;

WORDLISTS>


\ ---------- Primitive and block generation ----------
\ sd: these should be refactored to allow non-C primitives etc

<WORDLISTS ALSO CODE-GENERATOR ALSO DEFINITIONS

\ Generate the declarations for arguments and results
: GENERATE-PRIMITIVE-VARIABLES \ ( loc -- )
    DUP PRIMITIVE-ARGUMENTS
    BEGIN                                        \ arguments
	?DUP 0<>
    WHILE
	    DUP STRING-ELEMENT@
	    2 PICK <ELEMENT STRING-LIST-CONTAINS-BEFORE NOT IF
		S" CELL " TYPE
		DUP STRING-ELEMENT@ TYPE
		S" ;" TYPE CR
	    THEN
	    ELEMENT>
    REPEAT

    DUP PRIMITIVE-ARGUMENTS DUP 0<> IF          \ results
	ELEMENT>>
    THEN
    SWAP PRIMITIVE-RESULTS
    BEGIN
	?DUP 0<>
    WHILE
	    DUP STRING-ELEMENT@                                        \ args res addr n
	    2DUP 5 PICK STRING-LIST-CONTAINS-BEFORE NOT IF             \ args res addr n 
		2 PICK <ELEMENT STRING-LIST-CONTAINS-BEFORE NOT IF              \ args res
		    S" CELL " TYPE
		    DUP STRING-ELEMENT@ TYPE
		    S" ;" TYPE CR
		THEN
	    ELSE
		2DROP
	    THEN
	    ELEMENT>
    REPEAT
    DROP ;

\ Generate the stack pops for the arguments
: GENERATE-PRIMITIVE-ARGUMENT-POPS \ ( loc -- )
    PRIMITIVE-ARGUMENTS ?DUP 0<> IF
	ELEMENT>>
	BEGIN
	    ?DUP 0<>
	WHILE
		DUP STRING-ELEMENT@ ?DUP 0> IF
		    TYPE
		    SPACE S" = POP_CELL();" TYPE CR
		ELSE
		    DROP
		THEN
		<ELEMENT
	REPEAT
    THEN ;

\ Generate the stack pushes for the results
: GENERATE-PRIMITIVE-RESULT-PUSHES \ ( loc -- )
    PRIMITIVE-RESULTS
    BEGIN
	?DUP 0<>
    WHILE
	    S" PUSH_CELL(" TYPE
	    DUP STRING-ELEMENT@ TYPE
	    S" );" TYPE CR
	    ELEMENT>
    REPEAT ;

\ Generate the C source code for a primitive
: GENERATE-PRIMITIVE \ ( loc -- )
    DUP S" // " TYPE PRIMITIVE-NAME TYPE CR
    S" VOID" TYPE CR
    DUP PRIMITIVE-PRIMNAME TYPE S" ( XT _xt ) {" TYPE CR
    DUP GENERATE-PRIMITIVE-VARIABLES
    DUP GENERATE-PRIMITIVE-ARGUMENT-POPS
    DUP PRIMITIVE-TEXT TYPE CR
    DUP GENERATE-PRIMITIVE-RESULT-PUSHES
    S" }" TYPE CR CR
    DROP ;

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

WORDLISTS>
