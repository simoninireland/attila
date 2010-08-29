\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
\ All rights reserved.
\
\ Attila is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.
\
\ Attila is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ High-level image initialisation and finalisation
\
\ These routines initialise and finalise the image at the Attila level,
\ rather than at the image level.

\ Initialise the image
: INITIALISE-IMAGE
    (INITIALISE-IMAGE)
    
    \ initialise stacks and TIB
    HERE (DATA-STACK) A!           \ data stack 
    DATA-STACK-SIZE CELLS   ALLOT
    HERE (RETURN-STACK) A!         \ return stack 
    RETURN-STACK-SIZE CELLS ALLOT
    HERE TIB A!                    \ terminal input buffer
    TIB-SIZE                ALLOT ;

\ Finalise the image prior to being output
: FINALISE-IMAGE
    (FINALISE-IMAGE)

    \ patch the image's core variables
    TOP    (TOP)  A!      \ store TOP into image
    LASTXT LAST   XT! ;   \ store last xt defined
