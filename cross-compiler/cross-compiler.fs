\ $Id$

\ The Attila cross-compiler


\ ---------- More precise word list handling ----------

\ Find a word in a given wordlist, aborting if we fail
: FIND-IN-WORDLIST \ ( addr n wid -- xt )
    >R 2DUP R> SEARCH-WORDLIST
    0<> IF
	ROT 2DROP
    ELSE
	TYPE SPACE
	S" not found in word list" ABORT
    THEN ;

\ Immediate look-up in a specific wordlist
: ['WORDLIST] \ ( wid "name" -- xt )
    PARSE-WORD -ROT FIND-IN-WORDLIST ; IMMEDIATE

\ Look-up and execute/compile a word from (only) the given word list
\ sd: This may be a common pattern: should it be moved into wordlists.fs?
:NONAME \ ( wid "name" -- )
    POSTPONE ['WORDLIST]
    EXECUTE ;
:NONAME \ ( wid "name" -- )
    POSTPONE ['WORDLIST]
    DUP IMMEDIATE? IF
	EXECUTE     \ execute any immediate words as normal
    ELSE
	CTCOMPILE,  \ compile normal words
    THEN ;
INTERPRET/COMPILE [WORDLIST]

    
\ ---------- Helpers ----------

\ Placeholder for C-level inclusions from architecture description
: CINCLUDE \ ( "name" -- )
    PARSE-WORD 2DROP ;


\ ---------- The word lists ----------

\ Placed in ROOT so that we can access them even when we're not
\ searching FORTH
<WORDLISTS ALSO ROOT DEFINITIONS

NAMED-WORDLIST TARGET             CONSTANT TARGET-WORDLIST              \ locators
NAMED-WORDLIST CROSS              CONSTANT CROSS-WORDLIST               \ image manipulation
NAMED-WORDLIST CROSS-COMPILER     CONSTANT CROSS-COMPILER-WORDLIST      \ cross-compiler IMMEDIATE words
NAMED-WORDLIST CODE-GENERATOR     CONSTANT CODE-GENERATOR-WORDLIST      \ code generator

TARGET-WORDLIST             PARSE-WORD TARGET             (VOCABULARY)
CROSS-WORDLIST              PARSE-WORD CROSS              (VOCABULARY)
CROSS-COMPILER-WORDLIST     PARSE-WORD CROSS-COMPILER     (VOCABULARY)
CODE-GENERATOR-WORDLIST     PARSE-WORD CODE-GENERATOR     (VOCABULARY)

\ Look-up and execute/compile the next word from the given wordlist
: [TARGET]             TARGET-WORDLIST              POSTPONE [WORDLIST]  ; IMMEDIATE
: [CROSS]              CROSS-WORDLIST               POSTPONE [WORDLIST]  ; IMMEDIATE
: [CROSS-COMPILER]     CROSS-COMPILER-WORDLIST      POSTPONE [WORDLIST]  ; IMMEDIATE
: [CODE-GENERATOR]     CODE-GENERATOR-WORDLIST      POSTPONE [WORDLIST]  ; IMMEDIATE
: [FORTH]              FORTH-WORDLIST               POSTPONE [WORDLIST]  ; IMMEDIATE

\ Look-up the next word in the input stream from the given wordlist
: 'TARGET              TARGET-WORDLIST              POSTPONE ['WORDLIST] ;
: 'CROSS               CROSS-WORDLIST               POSTPONE ['WORDLIST] ;
: 'CROSS-COMPILER      CROSS-COMPILER-WORDLIST      POSTPONE ['WORDLIST] ;
: 'FORTH               FORTH-WORDLIST               POSTPONE ['WORDLIST] ;

\ Display the key wordlists alone, for debugging
: TARGET-WORDS             TARGET-WORDLIST             .WORDLIST ;
: CROSS-WORDS              CROSS-WORDLIST              .WORDLIST ;
: CROSS-COMPILER-WORDS     CROSS-COMPILER-WORDLIST     .WORDLIST ;
: CODE-GENERATOR-WORDS     CODE-GENERATOR-WORDLIST     .WORDLIST ;
WORDLISTS>


\ ---------- The main body of the cross-compiler ----------

<WORDLISTS ONLY FORTH ALSO CODE-GENERATOR ALSO DEFINITIONS
\ Current version string
: VERSION S" v1 $Date$" ;

\ Generate a timestamp string
: TIMESTAMP
    ." Attila cross-compiler " VERSION TYPE ;
WORDLISTS>

<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
\ Pre-defining unavoidable forward references
DEFER >CFA
DEFER (HEADER,)
DEFER CTCOMPILE,
DEFER NEXT,

\ Sizes of architectural elements, and other characteristics. These are
\ defaults that can be overridden later
4 VALUE /CELL
1 VALUE /CHAR
1 VALUE BIGENDIAN?
: CELLS \ ( n -- bs )
    /CELL * ;

.( Loading image manager...)
include c-image-fixedsize.fs    \ sd: should come from elsewhere

.( Loading target vm description...)
: CONSTANT
    CREATE [FORTH] ,
  DOES> [FORTH] @ ;
: USER
    CREATE [FORTH] ,
  DOES> [FORTH] @ [CROSS] USERVAR ;

include vm.fs
WORDLISTS>

.( Loading locator structures...)
<WORDLISTS ONLY FORTH ALSO DEFINITIONS
include cross-compiler/cross-locator.fs
WORDLISTS>

.( Loading cross-compiler memory model...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
include cross-compiler/cross-flat-memory-model.fs
WORDLISTS>

<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
: IMMEDIATE?    GET-STATUS IMMEDIATE-MASK    AND 0<> ;
: REDIRECTABLE? GET-STATUS REDIRECTABLE-MASK AND 0<> ;
WORDLISTS>

.( Loading C language primitive compiler...)
<WORDLISTS ONLY FORTH ALSO DEFINITIONS
include cross-compiler/cross-c.fs
WORDLISTS>

.( Loading vm description generator...)
<WORDLISTS ONLY FORTH ALSO CODE-GENERATOR ALSO DEFINITIONS
include cross-compiler/cross-vm.fs
WORDLISTS>

.( Loading cross-compiler file generators... )
<WORDLISTS ONLY FORTH ALSO CODE-GENERATOR ALSO DEFINITIONS
include cross-compiler/cross-generate.fs
WORDLISTS>

.( Loading cross-compiler high-level image initialisation...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO CODE-GENERATOR ALSO DEFINITIONS
include cross-compiler/cross-initialisation.fs
WORDLISTS>

.( Loading colon cross-compiler...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO CROSS-COMPILER DEFINITIONS
include cross-compiler/cross-colon.fs

: CONSTANT
    CREATE IMMEDIATE [FORTH] ,
  DOES> [FORTH] @
    INTERPRETING? NOT IF
	[ 'CROSS-COMPILER LITERAL [FORTH] CTCOMPILE, ]
    THEN ;
: USER
    CREATE IMMEDIATE [FORTH] ,
  DOES> [FORTH] @ [CROSS] USERVAR
    INTERPRETING? NOT IF
	[ 'CROSS-COMPILER ALITERAL [FORTH] CTCOMPILE, ]
    THEN ;
WORDLISTS>

.( Loading cross-compiler coding environments...)
<WORDLISTS ONLY FORTH ALSO ROOT DEFINITIONS
include cross-compiler/cross-environments.fs
WORDLISTS>

.( Loading cross-compiler comment handling...)
<WORDLISTS ONLY FORTH ALSO CROSS-COMPILER DEFINITIONS
include comments.fs
WORDLISTS>

.( Cross-compiler loaded)
