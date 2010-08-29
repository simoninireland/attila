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

\ Cross-compiler environments
\
\ These words provide an easier way of setting0up the various modes of
\ loading code into the cross-compiler. Essentially these boil down
\ to four options:
\
\ <CROSS ... CROSS>
\ Code that manipulates the target image directly
\
\ <CROSS-COMPILER ... CROSS-COMPILER>
\ Code providing defining words that run on the host but create code
\ on the target, like control structures
\
\ <CODE-GENERATOR ... CODE-GENERATOR>
\ Word for outputting the generated image ready for compilation
\
\ <TARGET ... TARGET>
\ Code that is being cross-compiled to the target. Note that this
\ environment relies on other features, and all code must be in general
\ be included from files rather than appearing in-line

\ Load code to manipulate the image only. Words can be defined in terms of
\ other image-manipulation words
: <CROSS
    <WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS ;
: CROSS> WORDLISTS> ;

\ Load code into the code cross-compiler. Words do not have access to other
\ cross-compiler words, but will change the target image.
: <CROSS-COMPILER
    [CROSS-COMPILER] HIDE-COLON-CROSS-COMPILER
    <WORDLISTS ONLY FORTH ALSO CROSS ALSO CROSS-COMPILER DEFINITIONS ;
: CROSS-COMPILER>
    [CROSS-COMPILER] UNHIDE-COLON-CROSS-COMPILER
    WORDLISTS> ;

\ Load code into the code generator
: <CODE-GENERATOR
    <WORDLISTS ONLY FORTH ALSO CODE-GENERATOR ALSO DEFINITIONS ;
: CODE-GENERATOR> WORDLISTS> ;

\ Load code into the image. This will cause the CROSS-COMPILER version of INCLUDE
\ to be used, which includes a cleverer executive that understand cross-compiling
\ sd: omitting FORTH omits immediate-mode arithmetic etc, which may be too strong
: <TARGET
    <WORDLISTS ONLY ALSO CROSS ALSO CROSS-COMPILER ALSO TARGET DEFINITIONS ;
: TARGET> WORDLISTS> ;

