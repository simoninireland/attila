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

\ Word headers, for indirected threading, with no names stored. Intended
\ for embedded and turnkey systems that won't interact


\ ---------- Header creation ----------

\ Create a header for the named word with the given code field. The
\ name is ignored
: (WORD) \ ( addr n cf -- xt )
    ROT 2DROP                \ dump the name
    CALIGNED                 \ align TOP on the next cell boundary 
    0 CCOMPILE,              \ status byte
    CALIGNED
    LASTXT ACOMPILE,         \ compile the link pointer
    TOP                      \ the xt
    R> CFACOMPILE,           \ the code pointer
    DUP LAST XT! ;           \ update LAST

\ Can't find words
\ sd: include or remove?
: (FIND) ( addr n x -- 0 )
    DROP 2DROP FALSE ;

