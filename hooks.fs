\ $Id$

\ Hooks
\
\ Hooks collect together a list of words that can be changed dynamically.
\ The idea is that a hook collects together the words that should happen
\ a particular "strategic" point -- for exaple when a file is loaded,
\ or when a word is defined -- and lets different parts of the system
\ specify some behaviour to be performed then.
\
\ Given the dynamic nature of a hook, hooked words shouldn't make
\ any assumptions about the order of their execution.
\
\ Hooks allocate a static amount of space for their hooked words, which
\ may be a little wasteful but is efficient. A linked-list representation
\ might save a little memory but would be more complicated to manage.

\ The number of words that can be hung from a hook -- modify with care
10 VALUE MAX-HOOKED-WORDS

\ Create a named hook
: (HOOK) \ ( addr n -- )
    (CREATE)
    MAX-HOOKED-WORDS DUP ,
    0 DO
	0 ,
    LOOP
    DOES>
      1 CELLS + ;

\ Create a hook from the next word
: HOOK \ ( "name" -- )
    PARSE-WORD (HOOK) ;

\ Add a word to a hook. The same word may appear more than once on a hook
: ADD-TO-HOOK \ ( xt addr -- ) 
    DUP 1 CELLS - @            \ xt addr n
    0 DO
	DUP @ 0= IF            \ xt addr
	    !
	    0 0 LEAVE
	ELSE
	    1 CELLS +
	THEN
    LOOP
    DROP 0<> IF
	S" Hook full" ABORT
    THEN ;

\ Remove a word from a hook. Removes only one instance, if there
\ are several
: REMOVE-FROM-HOOK \ ( xt addr -- )
    DUP 1 CELLS - @            \ xt addr n
    0 DO
	2DUP @ = IF            \ xt addr
	    0 SWAP !
	    0 LEAVE
	ELSE
	    1 CELLS +
	THEN
    LOOP
    2DROP ;

\ Run a hook
: RUN-HOOK \ ( addr -- )
    DUP 1 CELLS - @ 0 DO
	DUP @ ?DUP 0<> IF
	    EXECUTE
	THEN
	1 CELLS +
    LOOP
    DROP ;
