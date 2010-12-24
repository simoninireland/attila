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

\ Tests of hooks

TESTCASES" Hooks"

TESTING" Basic hook operations"

HOOK HOOK1
: HOOK-CONTINUE-1 1 FALSE ;
: HOOK-CONTINUE-2 2 FALSE ;
: HOOK-STOP TRUE ;

{ HOOK1 RUN-HOOK -> FALSE }
{ ' HOOK-CONTINUE-1 HOOK1 ADD-TO-HOOK
  HOOK1 RUN-HOOK -> 1 FALSE }
{ ' HOOK-CONTINUE-2 HOOK1 ADD-TO-HOOK
  HOOK1 RUN-HOOK -> 1 2 FALSE }


TESTING" Hook escapes"

{ ' HOOK-STOP       HOOK1 ADD-TO-HOOK
  ' HOOK-CONTINUE-2 HOOK1 ADD-TO-HOOK
  HOOK1 RUN-HOOK -> 1 2 TRUE }
