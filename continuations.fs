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

\ User-level continuations
\
\ These words provide user-level continuation semantics as well as
\ extension of the basic mechanisms.
\
\ Continuations are just blocks of memory, for example allocated
\ with DATA and ALLOT. They are used to persist the state of the system
\ such that it can be restored later. Continutions can be stored,
\ passed around, and then re-started zero, one or many times.
\
\ CALL-CC fills-in the continuation buffer passed to it and then invokes
\ a word, passing it the continuation. CALL-CONTINUATION is used to
\ re-start the continuation.
\
\ The two hooks hold words that are used to persist and restore additional
\ parts of the Attila system, for example floating-point stacks. A word
\ hung on a hook should have the stack picture ( cont -- acont ) and should
\ store (retrieve) the data it manages to (from) cont, returning acont as
\ the next free memory location. The hook words are run after the main
\ stacks have been saved (restored). This is obviously a source of
\ considerable instability unles done with care.

\ Hooks for extending the capture/restore behaviour
HOOK CAPTURE-CONTINUATION-HOOK
HOOK RESTORE-CONTINUATION-HOOK


\ Capture the current continuation and pass it on top of the stack
\ to the given word
: CALL-CC ( cont xt -- )
    SWAP (CAPTURE-STACKS)
    CAPTURE-CONTINUATION-HOOK RUN-HOOK 2DROP
    SWAP EXECUTE ;

\ Run the given continuation, restoring the system to the state it was
\ at when the continuation was captured
: RUN-CONTINUATION ( cont -- )
    (RESTORE-STACKS)
    RESTORE-CONTINUATION-HOOK RUN-HOOK 2DROP ;



    