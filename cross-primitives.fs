\ $Id$

\ The primitive cross-compiler
\
\ Primitives are stored in Attila source files alongside Attila code.
\ When loaded into the cross-compiler they are converted into
\ stylised C functions and output to a file ready for later compilation.
\
\ This code performs the same functions as tools/primgen does for
\ bootstrapping Attila.

\ ---------- Constants and data structures ----------

\ Create a primitive locator
\ Primitives are stored as lists of counted strings separated by blank
\ string:
\    counted string name
\    counted string primitive (C-level) name
\    list of arguments
\    list of results
\    counted string primitive code text
: (PRIMITIVE-LOCATOR) \ ( addr n -- )
    (DATA)
    0 ,    \ address of list of arguments
    0 ,    \ address of list of results
    0 , ;  \ address of code text

\ Compile the various elements in the locator, fixing its pointers. These
\ *must* be called immediately after the locator is created using (PRIMITIVE-LOCATOR)
\ to build its body. See the definition of PRIMITIVE: below
: PRIMITIVE-NAME, \ ( addr n xt -- )
    DROP S, ;
: PRIMITIVE-PRIMNAME, \ ( addr n xt -- )
    DROP S, ;
: PRIMITIVE-ARGUMENTS, \ ( xt -- elem )
    NULLSTRING STRING-ELEMENT
    DUP -ROT >BODY ! ;
: PRIMITIVE-RESULTS, \ ( xt -- elem )
    NULLSTRING STRING-ELEMENT
    DUP -ROT >BODY 1 CELLS + ! ;
: PRIMITIVE-TEXT, \ ( addr n xt -- )
    HERE >R
    ROT S, R> SWAP
    >BODY 2 CELLS + ! ;

\ Return the primitive name
: PRIMITIVE-NAME \ ( loc -- addr n )
    3 CELLS + COUNT ;

\ Return the primitive primitive (C-level) name
: PRIMITIVE-PRIMNAME \ ( loc -- addr n )
    3 CELLS + COUNT + COUNT ;
    
\ Return lists of parameters and results
: PRIMITIVE-ARGUMENTS \ ( loc -- elem )
    @ ELEMENT> ;
: PRIMITIVE-RESULTS \ ( loc -- elem )
    1 CELLS + @ ELEMENT> ;

\ Return the primitive code text
: PRIMITIVE-TEXT \ ( loc -- addr n )
    2 CELLS + @ COUNT ;


\ ---------- Some handy helper words ----------

\ Test whether two strings are equal, leaving a flag and the first string
: S=? \ ( addr1 n1 addr2 n2 -- add1 n1 f )
    2OVER S= ;

\ Move a counted string to a memory address, on the assumption that there
\ is sufficient space
: SMOVE \ ( addr1 n addr2 -- )
    2DUP C!
    1+ SWAP CMOVE ;

\ Return the address of the character after the end of the given counted string
: CHARACTER-AFTER \ ( caddr -- addr )
    COUNT CHARS + ;

\ Catenate the first string to the second (counted) string
: S+ \ ( addr n caddr -- )
    DUP CHARACTER-AFTER     \ addr n caddr addr2
    ROT                     \ addr addr2 n caddr
    DUP C@                  \ addr addr2 n caddr oldn
    2 PICK + SWAP C!
    CMOVE ;

\ Increment a string count
: C1+! \ ( addr -- )
    DUP C@ 1+ SWAP C! ;

\ Catenate newline to the (counted) string
: SNL+ \ ( caddr -- )
    NL OVER CHARACTER-AFTER C!
    C1+! ;

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


\ ---------- Primitive name generator ----------

VARIABLE (#PRIM) 0 (#PRIM) !    \ uniques
DATA (PRIMNAME) 32 ALLOT       \ buffer for name

\ Create a new primitive name
: PRIMNAME \ ( -- addr n )
    S" prim" (PRIMNAME) SMOVE
    (#PRIM) @ <# #S #> (PRIMNAME) S+
    1 (#PRIM) +! 
    (PRIMNAME) COUNT ;


\ ---------- Primitive parser ----------

\ Buffer for the text
DATA (PRIMTEXT) 2048 ALLOT

\ Parse a primitive, creating a locator word that holds all its details
: PRIMITIVE:
    PARSE-WORD 2DUP
    (PRIMITIVE-LOCATOR)            \ compile a locator
    LASTXT PRIMITIVE-NAME,         \ compile the primitive's name
    
    PARSE-WORD S" (" S=? IF        \ compile primitive's primitive name, creating one if necessary
	2DROP PRIMNAME
    THEN
    LASTXT PRIMITIVE-PRIMNAME,

    LASTXT PRIMITIVE-ARGUMENTS,    \ compile the parameters list
    BEGIN
	PARSE-WORD S" --" S=? NOT
    WHILE
	    STRING-ELEMENT DUP -ROT ELEMENT+
    REPEAT
    2DROP DROP

    LASTXT PRIMITIVE-RESULTS,      \ compile the results list
    BEGIN
	PARSE-WORD S" )" S=? NOT
    WHILE
	    STRING-ELEMENT DUP -ROT ELEMENT+
    REPEAT
    2DROP DROP

    0 (PRIMTEXT) C!               \ compile the text
    (PRIMTEXT)
    BEGIN
	DUP
	REFILL IF
	    SOURCE 1- S" ;PRIMITIVE" S=? IF
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
	    -ROT S+
	    DUP SNL+
    REPEAT
    COUNT LASTXT PRIMITIVE-TEXT, ;


\ ---------- Primitive generation ----------

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
	    2DUP 4 PICK <ELEMENT STRING-LIST-CONTAINS-BEFORE NOT IF    \ args res addr n 
		3 PICK STRING-LIST-CONTAINS-BEFORE NOT IF              \ args res
		    S" CELL " TYPE
		    DUP STRING-ELEMENT@ TYPE
		    S" ;" TYPE CR
		THEN
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
    S" VOID" TYPE CR
    DUP PRIMITIVE-NAME TYPE S" ( XT _xt ) {" TYPE CR
    DUP GENERATE-PRIMITIVE-VARIABLES
    DUP GENERATE-PRIMITIVE-ARGUMENT-POPS
    DUP PRIMITIVE-TEXT TYPE CR
    DUP GENERATE-PRIMITIVE-RESULT-PUSHES
    S" }" TYPE CR CR
    DROP ;

