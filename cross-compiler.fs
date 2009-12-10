\ $Id$

\ The Attila cross-compiler
\

\ ---------- The word lists ----------

WORDLIST CONSTANT CROSS-WORDLIST                   \ Host words
CROSS-WORDLIST PARSE-WORD CROSS (VOCABULARY)
WORDLIST CONSTANT TARGET-WORDLIST                  \ Target locator words
TARGET-WORDLIST PARSE-WORD TARGET (VOCABULARY)

\ Find a word in a given wordlist, aborting if we fail
: FIND-IN-WORDLIST \ ( addr n wid -- xt )
    >R 2DUP R> SEARCH-WORDLIST
    0<> IF
	ROT 2DROP
    ELSE
	TYPE SPACE
	S" not found in word list" ABORT
    THEN ;

\ Look-up and execute/compile a word from (only) the given word list
\ (Compilation on the host, not the target) 
:NONAME \ ( wid "name" -- )
    PARSE-WORD -ROT FIND-IN-WORDLIST
    EXECUTE ;
:NONAME \ ( wid "name" -- )
    PARSE-WORD -ROT FIND-IN-WORDLIST
    XTCOMPILE, ;
INTERPRET/COMPILE [WORDLIST]
: [CROSS]  CROSS-WORDLIST  POSTPONE [WORDLIST] ; IMMEDIATE
: [TARGET] TARGET-WORDLIST POSTPONE [WORDLIST] ; IMMEDIATE


\ ---------- The main body of the cross-compiler ----------

\ Definitions go into CROSS, but it isn't being searched, so we
\ will have to escape all CROSS words explicitly with [CROSS]
ALSO CROSS DEFINITIONS

\ The target architecture's description
\ sd: should come from elsewhere
include x-arch-gcc-host.fs

\ Primitive parsing
include cross-primitives.fs

\ Return the number of bytes needed to hold n target cells
: CELLS \ ( n -- b )
    [CROSS] /CELL * ;

\ The image manager
\ sd: should come from elsewhere
include x-image-gcc.fs

\ Get ready to go
ONLY FORTH ALSO DEFINITIONS ALSO CROSS
(INITIALISE-IMAGE)


\ ---------- Running the cross-compiler ----------
\ sd: should come from elsewhere

\ Put the code we need to build the image into CROSS, where they will pick
\ up the CROSS (image) definitions for the core primitives
ONLY FORTH ALSO CROSS ALSO DEFINITIONS
include flat-memory-model.fs
include colon.fs

\ Now load the parts of the image into TARGET, using only the
\ words defined in CROSS and TARGET
\ sd: this might not be enough, and we might need FORTH too
ONLY CROSS ALSO TARGET ALSO DEFINITIONS

