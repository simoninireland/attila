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

\ The colon-definition compiler

\ ---------- Start and end hooks ----------

\ Hooks
HOOK (START-DEFINITION)
HOOK (END-DEFINITION) 

\ Brackets to run the start and end hooks, ignoring any bailout status
: START-DEFINITION (START-DEFINITION) RUN-HOOK DROP ;
: END-DEFINITION   (END-DEFINITION)   RUN-HOOK DROP ;


\ ---------- The colon-compiler ----------

\ The colon-definer
: : ( "name" -- xt )
    START-DEFINITION
    PARSE-WORD ['] (:) CFA@ (WORD)
    ] ;

\ Complete a colon-definition
: ; ( xt -- ) [ IMMEDIATE ] \ make ; immediate immediately, to ensure that the
                            \ shadow is used at the end of this definition
                            \ when cross-compiling
    ALIGNED NEXT,
    END-DEFINITION
    POSTPONE [ DROP ; ( IMMEDIATE )

