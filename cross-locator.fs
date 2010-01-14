\ $Id$

\ Locators
\
\ Locators are word in TARGET that represent words in the target
\ image for the host. Each target word has a locator that can be
\ used to retrieve its xt and (for primitives) its name and C text.

\ ---------- Basic locator ----------

\ Create a locator
: (LOCATOR) \ ( addr n -- )
    (DATA)
    0 , ;    \ target xt

\ Store the target xt
: TARGET-XT, \ ( txt loc -- )
    ! ; 

\ Return the target xt
: TARGET-XT \ ( loc -- txt )
    @ ;


\ ---------- Normal word locators ----------

\ Create a locator for the given target xt, leaving it on the stack
\ afterwards for convenience
: (WORD-LOCATOR) \ ( addr n txt -- txt )
    ROT (LOCATOR)
    DUP LASTXT EXECUTE ! ;


\ ---------- Primitive locators ----------

\ Primitive locators have extra structure, stored as lists
\ of counted strings separated by blank string:
\    target xt
\    counted string name
\    counted string primitive (C-level) name
\    list of arguments
\    list of results
\    null-terminated zstring primitive code text
: (PRIMITIVE-LOCATOR) \ ( addr n -- )
    (LOCATOR)
    0 ,    \ address of list of arguments
    0 ,    \ address of list of results
    0 ,    \ address of code text
    ;

\ Compile the various elements in the locator, fixing its pointers. These
\ *must* be called immediately after the locator is created using
\ (PRIMITIVE-LOCATOR) to build its body. See the definition
\ of PRIMITIVE: below
: PRIMITIVE-NAME, \ ( addr n loc -- )
    DROP S, ;
: PRIMITIVE-PRIMNAME, \ ( addr n loc -- )
    DROP S, ;
: PRIMITIVE-ARGUMENTS, \ ( loc -- elem )
    NULLSTRING STRING-ELEMENT
    DUP -ROT 1 CELLS + ! ;
: PRIMITIVE-RESULTS, \ ( loc -- elem )
    NULLSTRING STRING-ELEMENT
    DUP -ROT 2 CELLS + ! ;
: PRIMITIVE-TEXT, \ ( zaddr loc -- )
    HERE >R
    SWAP Z, R> SWAP
    3 CELLS + ! ;
    
\ Return the primitive name
: PRIMITIVE-NAME \ ( loc -- addr n )
    4 CELLS + COUNT ;

\ Return the CFA, which is the counted-string address of the primname
: PRIMITIVE-CFA \ ( loc -- caddr )
    4 CELLS + COUNT + ;

\ Return the primitive primitive (C-level) name
: PRIMITIVE-PRIMNAME \ ( loc -- addr n )
    4 CELLS + COUNT + COUNT ;

\ Return lists of parameters and results
: PRIMITIVE-ARGUMENTS \ ( loc -- elem )
    1 CELLS + @ ELEMENT> ;
: PRIMITIVE-RESULTS \ ( loc -- elem )
    2 CELLS + @ ELEMENT> ;

\ Return the primitive code text
: PRIMITIVE-TEXT \ ( loc -- addr n )
    3 CELLS + @ ZCOUNT ;
