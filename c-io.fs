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

\ Terminal I/O primitives using C library functions
\
\ This code only manages the really basic I/O actions of acquiring
\ characters from file or terminal streams. All parsing etc has
\ been refactored into Attila code to minimise the use of primitives.


\ ---------- Terminal I/O ----------

\ Read a line into the text buffer from the input source.
\ Leave a flag on the stack, 0 if the input source is exhausted
C: REFILL fill_tib ( -- f )
  FILE *input_source = (FILE *) (*(user_variable(USER_INPUTSOURCE)));
  char **tib = (char **) user_variable(USER_TIB);
  int *offset = (int *) user_variable(USER__IN);

  if(input_source == stdin)
    printf(" ok ");
  if(input_source == (FILE *) -1)
    f = FALSE;
  else {
    // read line from underlying file
    PUSH_CELL(*tib);
    PUSH_CELL(256);
    PUSH_CELL(input_source);
    CALL(prim_read_line);
    POP_CELL();   f = POP_CELL();   POP_CELL();
  }

  // reset input to the start of the TIB
  *offset = f ? 0 : -1;
;C
      
\ Test if the input source has been exhausted, i.e. we can't do a REFILL
C: EXHAUSTED? ( -- eof )
    FILE *f;

    f = (FILE *) (*(user_variable(USER_INPUTSOURCE)));
    eof = (f == ((FILE *) -1)) ? TRUE : feof(f);
;C

\ Place the address of the current input point in the TIB and the
\ number of remaining characters onto the stack, or 0 if none
C: SOURCE ( -- )
  char *tib;
  int offset;
  char *addr;
  int len;   
      
  tib = (char *) (*(user_variable(USER_TIB)));
  offset = (int) (*(user_variable(USER__IN)));
  if(offset == -1)
    PUSH_CELL(0);
  else {
    addr = tib + offset;
    len = strlen(addr);
    if(len == 0)
      PUSH_CELL(0);
    else {
      PUSH_CELL((CELL) addr);
      PUSH_CELL((CELL) len);
    }
  }
;C

\ Print a character on the output stream
C: EMIT ( c -- )
  FILE *output_sink = (FILE *) (*(user_variable(USER_OUTPUTSINK)));
	
  fprintf(output_sink, "%c", (CHARACTER) c);   fflush(output_sink);
;C






	    