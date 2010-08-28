\ $Id$

\ File generators

\ Generate a VM description from the given file
: GENERATE-VM \ ( addr n -- )
    >R >R
    <WORDLISTS ONLY FORTH ALSO CODE-GENERATOR
    R> R>
    INCLUDED
    WORDLISTS> ;

\ Generate the image from a template file
: GENERATE-IMAGE \ ( -- )
    ." // " TIMESTAMP CR CR

    ." // VM definition" CR
    S" vm.fs" GENERATE-VM CR CR

    ." // Architecture definition" CR
    ." #include " QUOTES ." x-host.h" QUOTES CR CR

    ." // Primitives table" CR
    GENERATE-PRIMITIVE-DECLARATIONS CR CR

    ." // C support" CR
    GENERATE-CBLOCKS CR CR

    ." // Primitives" CR
    GENERATE-PRIMITIVES CR CR

    ." // Initial image" CR
    EMIT-IMAGE CR CR ;


\ Save the image to the given file
: SAVE-IMAGE-AS \ ( "name" -- )
    PARSE-WORD 2DUP W/O CREATE-FILE IF
	DROP
	TYPE SPACE S" cannot be opened" ABORT
    ELSE
	ROT 2DROP
	(<TO)
	GENERATE-IMAGE
	TO>
    THEN ;
