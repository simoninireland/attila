\ $Id$

\ Handle C-language structures when encountered in "normal" behaviour when
\ not cross-compiling.
\
\ Since we can't change the VM on the fly, we simply check
\ whether there's a word with the given name in the search
\ order and die if there isn't. This allows any normal words to
\ compile: it doesn't, of course, guarantee that the word we find
\ is the same as the primitive we expect.
\
\ See cross-c.fs for a cross-compiler view of primitives.

\ ---------- C language primitive parser ----------

\ Parse a primitive and check for its existence at Attila level
: C:
    PARSE-WORD 2DUP TYPE CR 2DUP FIND IF
	2DROP
    ELSE
	\ no word with this name, abort
	TYPE S" ?" ABORT
    THEN

    BEGIN
	REFILL IF
	    SOURCE 1- S" ;C" S=CI IF
		REFILL DROP
		FALSE
	    ELSE
		TRUE
	    THEN
	ELSE
	    S" Input ended before ;C" ABORT
	THEN
    WHILE
    REPEAT ;

\ Ignore any C headers
: CHEADER:
    BEGIN
	REFILL IF
	    SOURCE 1- S" ;CHEADER" S=CI IF
		REFILL DROP
		FALSE
	    ELSE
		TRUE
	    THEN
	ELSE
	    S" Input ended before ;CHEADER" ABORT
	THEN
    WHILE
    REPEAT ;
