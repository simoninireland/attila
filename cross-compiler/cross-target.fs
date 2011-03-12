\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

\ VM description for the cross-compiler
\
\ The words in this file are used to set parameters for the cross-compiler,
\ which is then loaded by a driver file.

\ Target VM parameters
RECORD: CROSS-COMPILER-TARGET-DESCRIPTION
    100 CHARS FIELD: TARGET-NAME           \ target (architecture) name
    256 CHARS FIELD: TARGET-VM             \ filename for VM description
    256 CHARS FIELD: TARGET-MAIN           \ main file, usually C
    256 CHARS FIELD: TARGET-INCLUDE-PATH   \ initial include path
    256 CHARS FIELD: TARGET-OUTPUT-FILE    \ file to write the image to
;RECORD

\ The target VM description
CROSS-COMPILER-TARGET-DESCRIPTION CROSS-COMPILER-TARGET

\ Setters, used by top-level driver
: SET-CROSS-COMPILER-TARGET-NAME         CROSS-COMPILER-TARGET TARGET-NAME         SMOVE ;
: SET-CROSS-COMPILER-TARGET-VM-FILE      CROSS-COMPILER-TARGET TARGET-VM           SMOVE ;
: SET-CROSS-COMPILER-TARGET-MAIN-FILE    CROSS-COMPILER-TARGET TARGET-MAIN         SMOVE ;
: SET-CROSS-COMPILER-TARGET-INCLUDE-PATH CROSS-COMPILER-TARGET TARGET-INCLUDE-PATH SMOVE ;
: SET-CROSS-COMPILER-OUTPUT-FILE         CROSS-COMPILER-TARGET TARGET-OUTPUT-FILE  SMOVE ;

\ Getters, used in the cross-compiler itself
: CROSS-COMPILER-TARGET-NAME         CROSS-COMPILER-TARGET TARGET-NAME         COUNT ;
: CROSS-COMPILER-TARGET-VM-FILE      CROSS-COMPILER-TARGET TARGET-VM           COUNT ;
: CROSS-COMPILER-TARGET-MAIN-FILE    CROSS-COMPILER-TARGET TARGET-MAIN         COUNT ;
: CROSS-COMPILER-TARGET-INCLUDE-PATH CROSS-COMPILER-TARGET TARGET-INCLUDE-PATH COUNT ;
: CROSS-COMPILER-OUTPUT-FILE         CROSS-COMPILER-TARGET TARGET-OUTPUT-FILE  COUNT ;

