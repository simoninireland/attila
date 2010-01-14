\ $Id$

\ Terminal I/O primitives using host gcc
\ sd: should be refactored into Attila once debugged


\ ---------- Terminal I/O ----------

\ Read a line into the text buffer from the input source. If the input
\ source is a terminal, use readline(); otherwise, just read the line.
\ Leave a flag on the stack, 0 if the input source is exhausted
C: REFILL fill_tib ( -- f )
  FILE *input_source = (FILE *) (*(user_variable(USER_INPUTSOURCE)));
  char **tib = (char **) user_variable(USER_TIB);
  int *offset = (int *) user_variable(USER_OFFSET);

  if(input_source == stdin)
    printf(" ok ");

  // read line from underlying file
  PUSH_CELL(*tib);
  PUSH_CELL(256);
  PUSH_CELL(input_source);
  prim_read_line(_xt);
  POP_CELL();   f = POP_CELL();   POP_CELL();

  // reset input to the start of the TIB
  *offset = f ? 0 : -1;
;C


\ Place the address of the current input point in the TIB and the
\ number of remaining characters onto the stack
C: SOURCE ( -- addr n )
  char *tib;
  int offset;
      
  tib = (char *) (*(user_variable(USER_TIB)));
  offset = (int) (*(user_variable(USER_OFFSET)));
  addr = (CELL) ((offset == -1) ? tib : tib + offset);
  n = (CELL) ((offset == -1) ? 0 : strlen((char *) addr));
;C


\ Parse the next word, consuming leading whitespace. Leave either
\ an address count pair or zero on the stack
C: PARSE-WORD ( -- )
  CELL c = 0;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  int newoffset;
  int len;
  char *newpoint;
  CELL remaining;

  do {
    // fill the TIB if we need a line
    if(*offset == -1) {
      fill_tib(_xt);

      // check for an exhausted input source, and fail if we have one
      remaining = POP_CELL();
      if(!remaining) {
	PUSH_CELL(remaining);
	return;
      }
    }
    
    // consume leading whitespace
    len = strspn(tib + *offset, WHITESPACE);
    *offset += len;
  
    // parse everything except whitespace
    len = strcspn(tib + *offset, WHITESPACE);
    if(len == 0)
      *offset = -1;
  } while(len == 0);
  newoffset = *offset + len;
  if(*(tib + newoffset) == '\0')
    newoffset = -1;
  
  // push the result onto the stack
  newpoint = tib + *offset;
  PUSH_CELL(newpoint);
  PUSH_CELL(len);

  // update point
  *offset = newoffset;
;C


\ Consume all instances of the delimiter character in the input line
C: CONSUME ( c -- )
  CELL remaining;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  char cc;

  // fill the TIB if we need a line
  if(*offset == -1) {
    fill_tib(_xt);

    // check for an exhausted input source
    remaining = POP_CELL();
    if(!remaining)
      return;
  }

  while(cc = *(tib + *offset),
	((cc != '\0') &&
	 (cc == (char) c)))
    (*offset)++;
;C


\ Parse the input stream up to the next instance of the delimiter
\ character, returning null if there is no such delimiter before
\ the end of the line
C: PARSE ( c -- )
  char *ptr, *end;
  int len;
  CELL remaining;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  char *point;

  // fill the TIB if we need a line
  if(*offset == -1) {
    fill_tib(_xt);

    // check for an exhausted input source, and fail if we have one
    remaining = POP_CELL();
    if(!remaining) {
      PUSH_CELL(remaining);
      return;
    }
  }

  // search for the next delimiter character
  point = tib + *offset;
  ptr = index(point, c);
  if(ptr == NULL) {
    PUSH_CELL(ptr);
    return;
  }

  // push the result onto the stack
  len = ptr - point;
  PUSH_CELL(point);
  PUSH_CELL(len);

  // update point
  *offset += len + 1;
  if(*(tib + *offset) == '\0')
    *offset = -1;
;C

\ Print the given string on the terminal
C: TYPE ( addr n -- )
  FILE *output_sink = (FILE *) (*(user_variable(USER_OUTPUTSINK)));
  char *buf;
	
  // copy string to null-terminated local buffer
  buf = (CHARACTERPTR) malloc(n + 1);
  memcpy(buf, (BYTEPTR) addr, n);   buf[n] = '\0';

  // print and tidy up
  fprintf(output_sink, "%s", buf);   fflush(output_sink);
  free(buf);
;C

\ Print a character on the terminal
C: EMIT ( c -- )
  FILE *output_sink = (FILE *) (*(user_variable(USER_OUTPUTSINK)));
	
  fprintf(output_sink, "%c", (CHARACTER) c);   fflush(output_sink);
;C

\ Print the top number on the stack
C: . ( n -- )
  FILE *output_sink = (FILE *) (*(user_variable(USER_OUTPUTSINK)));

  fprintf(output_sink, "%d", n);   fflush(output_sink);
;C

\ Print the whole stack
C: .S prim_dot_s ( -- )
    FILE *output_sink = (FILE *) (*(user_variable(USER_OUTPUTSINK)));
    int i, n;

    n = DATA_STACK_DEPTH();
    fprintf(output_sink, "#%d", n);
    for(i = n - 1; i >= 0; i--) {
        fprintf(output_sink, " %d", *(DATA_STACK_ITEM(i)));
    }
    fflush(output_sink);
;C


\ ---------- Re-starting ----------

\ Abort to a warm-start with an error message
: ABORT \ ( addr n -- )
    TYPE WARM ;

	
\ ---------- Compilation support ----------

\ Convert a token to a number if possible, pushing the result
\ (if done) and a flag
C: NUMBER? ( addr n -- )
  char *buf;
  char *digitptr;
  int i, len, acc, digit;
  int valid = 1;
  int base = *(user_variable(USER_BASE));

  buf = (CHARACTERPTR) malloc(n + 1);
  memcpy(buf, addr, n);   buf[n] = '\0';

  // convert number
  acc = 0;
  for(i = 0; i < n; i++) {
    digitptr = index(digits, toupper(buf[i]));
    digit = digitptr - digits;
    if((digitptr == NULL) ||
       (digit > base)) {
      // illegal number character
      valid = 0;
      break;
    }
    acc = (acc * base) + digit;
  }

  if(valid)
    PUSH_CELL(acc);
  PUSH_CELL(valid);
  free(buf);
;C

\ Test if the input source has been exhausted, i.e. we can't do a REFILL
C: EXHAUSTED? ( -- eof )
    FILE *f;

    f = (FILE *) (*(user_variable(USER_INPUTSOURCE)));
    eof = feof(f);
;C





	    