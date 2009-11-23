\ $Id$

\ The Attila cross-compiler
\

\ ---------- The word lists ----------

VOCABUALRY CROSS   \ Host words
VOCABULARY TARGET  \ Target words as data

\ Execute a host cross word without changing the search order
\ State-smart: do the word or compile it
:NONAME \ ( "name" -- )
    ALSO CROSS
    '
    PREVIOUS
    EXECUTE ;
:NONAME \ ( "name" -- )
    ALSO CROSS
    '
    PREVIOUS
    COMPILE, ;
INTERPRET/COMPILE [CROSS]


\ ---------- Target image creation and access ----------

\ The body address of the current target image we're working with
VARIABLE (CURRENT-TARGET-IMAGE)

\ Fetch the currently selected image
: CURRENT-TARGET-IMAGE (CURRENT-TARGET-IMAGE) @ ;

\ Create a named target image with the given maximum size. Executing an
\ image selects it for compilation
: TARGET-IMAGE \ ( bs "name" -- )
    CREATE
    0 ,                \ the TOP pointer
    0 ,                \ the HERE pointer
    ALLOT              \ the memory
  DOES> \ ( addr -- )
    (CURRENT-TARGET-IMAGE) ! ;

\ Return the host address of the given logical target address
: TARGET> \ ( taddr -- addr )
    CURRENT-TARGET-IMAGE 2 CELLS + + ;

\ Update pointers having compiled or stored data
: COMPILED \ ( b -- )
    CURRENT-TARGET-IMAGE +! ;
: ALLOTTED \ ( b -- )
    CURRENT-TARGET-IMAGE 1 CELLS + +! ;


\ ---------- Cross-compiler compilation primitives ----------

\ Place definitions into CROSS without making it searchable, so
\ the words are defined in terms of the host
ALSO CROSS DEFINITIONS

\ Return the logical address of the top of the target's dictionary
: TOP \ ( -- taddr )
    CURRENT-TARGET-IMAGE @ ;

\ Return the logical address of the top of the target's data space
: HERE \ ( -- taddr )
    CURRENT-TARGET-IMAGE 1 CELLS + @ ;

\ Compile elements into the target's code
: CCOMPILE, ( c -- )
    TOP TARGET> C!
    1 COMPILED ;
: COMPILE, ( n -- )
    TOP TARGET> !
    1 CELLS COMPILED ;

\ Compile elements as target data
: , \ ( n -- )
    HERE TARGET> !
    1 CELLS COMPILED ;
: C, \ ( n -- )
    HERE TARGET> C!
    1 COMPILED ;

\ Host access to target image data
: !
    \ ( n addr -- )
    TARGET! ! ;
: @ \ ( addr -- n )
    TARGET> @ ;
: C!
    \ ( d addr -- )
    TARGET! C! ;
: C@ \ ( addr -- c )
    TARGET> C@ ;

\ Target compilation state
VARIABLE STATE




\ ---------- The colon-compiler ----------

\ Compile a locator word in TARGET that returns the target
\ address of the word
: (TARGET:) \ ( addr n -- )
    GET-CURRENT >R            \ save the current word list
    ALSO TARGET DEFINITIONS   \ place definition in TARGET
    (CREATE)                  \ named word
    [CROSS] TOP ,             \ body holds target address
    R> SET-CURRENT            \ restore current
  DOES> \ ( addr -- taddr )
    @ ;                       \ return the target-level address

\ The host cross-colon-compiler
: : \ ( "name" -- xt )
    PARSE-WORD
    2DUP (TARGET:)            \ create locator word
    [CROSS] (HEADER,)

    
    
    