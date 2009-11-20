\ $Id$

\ The Attila cross-compiler
\

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
    

\ ---------- The colon-compiler ----------

: TARGET-: \ ( "name" -- )
    PARSE-WORD TARGET-(HEADER,)
    


    
    
    