\ $Id$

\ VM description generator for cross-compiler

<WORDLISTS ALSO CODE-GENERATOR DEFINITIONS

\ Translate characters unsafe for C
: SAFE-IDENTIFIER \ ( addr n -- )
    2DUP
    S" -><+@;:[]{}()!?/"
    S" ________________" TRANSLATE ;

\ Parse a word and make it a safe C identifier
: PARSE-IDENTIFIER \ ( "name" -- addr n )
    PARSE-WORD [CODE-GENERATOR] SAFE-IDENTIFIER ;

\ The defining words used in the vm.fs and arch.fs files
: CONSTANT \ ( n "name" -- )
    [CODE-GENERATOR] PARSE-IDENTIFIER
    ." #define " TYPE SPACE . CR ;
: USER \ ( n "name" -- )
    [CODE-GENERATOR] PARSE-IDENTIFIER
    ." #define USER_" TYPE SPACE . CR ;
: C-INCLUDE \ ( "name" -- )
    PARSE-WORD
    ." #include " QUOTES TYPE QUOTES CR ;
    
WORDLISTS>
