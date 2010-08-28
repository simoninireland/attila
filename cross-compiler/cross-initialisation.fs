\ $Id$

\ High-level image initialisation and finalisation
\
\ These routines initialise and finalise the image at the Attila level, rathr
\ than at the image level.

\ Initialise the image
: INITIALISE-IMAGE
    (INITIALISE-IMAGE)
    
    \ initialise stacks and TIB
    HERE (DATA-STACK) A!           \ data stack 
    DATA-STACK-SIZE CELLS   ALLOT
    HERE (RETURN-STACK) A!         \ return stack 
    RETURN-STACK-SIZE CELLS ALLOT
    HERE TIB A!                    \ terminal input buffer
    TIB-SIZE                ALLOT
;

\ Finalise the image prior to being output
: FINALISE-IMAGE
    (FINALISE-IMAGE)
    
    TOP    (TOP)  A!      \ store TOP into image
    LASTXT LAST   XT!     \ store last xt defined
;
