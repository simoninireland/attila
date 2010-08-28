\ $Id$

\ Cross-compiler environments

\ Load code to manipulate the image only. Words can be defined in terms of
\ other image-manipulation words
: <CROSS
    <WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS ;
: CROSS> WORDLISTS> ;

\ Load code into the code cross-compiler. Words do not have access to other
\ cross-compiler words, but will change the target image.
: <CROSS-COMPILER
    [CROSS-COMPILER] HIDE-COLON-CROSS-COMPILER
    <WORDLISTS ONLY FORTH ALSO CROSS ALSO CROSS-COMPILER DEFINITIONS ;
: CROSS-COMPILER>
    [CROSS-COMPILER] UNHIDE-COLON-CROSS-COMPILER
    WORDLISTS> ;

\ Load code into the code generator
: <CODE-GENERATOR
    <WORDLISTS ONLY FORTH ALSO CODE-GENERATOR ALSO DEFINITIONS ;
: CODE-GENERATOR> WORDLISTS> ;

\ Load code into the image. This will cause the CROSS-COMPILER version of INCLUDE
\ to be used, which includes a cleverer executive that understand cross-compiling
\ sd: omitting FORTH omits immediate-mode arithmetic etc, which may be too strong
: <TARGET
    <WORDLISTS ONLY ALSO CROSS ALSO CROSS-COMPILER ALSO TARGET DEFINITIONS ;
: TARGET> WORDLISTS> ;

