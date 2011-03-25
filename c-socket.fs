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

\ Socket primitives
\
\ We restrict ourselves to INET domain sockets, since the other kinds
\ are encountered with vanishingly small probability. Socket operations
\ are all non-blocking.

CHEADER:
#include <errno.h>
#include <fcntl.h>
#include <sys/time.h>
#include <sys/select.h>
#include <sys/types.h>
#include <sys/socket.h>

;CHEADER

  
\ ---------- Socket types ----------

C: STREAM-SOCKET ( -- type )
   type = (CELL) SOCK_STREAM;
;C

C: DATAGRAM-SOCKET ( -- type )
   type = (CELL) SOCK_DGRAM;
;C

C: RAW-SOCKET ( -- type )
   type = (CELL) SOCK_RAW;
;C


\ ---------- Socket creation ----------

\ Create a new socket of the given type, returning an identifier and
\ an error code (0 for success)
C: (SOCKET) bracket_socket ( type -- s ior )
  s = CELL) socket(PF_INET, type, 0);
  ior = (s < 0) ? (CELL) errno : 0;
;C

\ Create a new server socket listening on the given address and port, returning
\ a socket and an error code (0 for success)      
C: CREATE-SERVER-SOCKET ( type ipaddr port -- s ior )
  int s;
  struct sockaddr addr;
  int i;
      
  // create the socket
  PUSH_CELL(type); 
  CALL(bracket_socket);
  ior = POP_CELL();   s = POP_CELL();
  if(ior == 0) {
    // bind to an address
    bzero((char *) &addr, sizeof(struct sockaddr));
    addr.sin_family = AF_INET;
    addr.sin_addr = &addr;
    addr.sin_port = htons(port);
    ior = bind(s, &addr, sizeof(struct sockaddr));
    if(ior < 0) {
      ior = errno;
    } else {
      // set a default listening queue -- fine for most purposes
      listen(s, 5);

      // make the socket non-blocking
      fcntl(s, F_SETFL, O_NONBLOCK);
    }
  }
;C
      
\ Accept a connection on a socket, non-blocking, with an addres structure that
\ is filled-in to identify the accepted client. Returns ( -- 0 0 ) in
\ the absence of error or connection, or ( -- fd 0 ) for a new connection
C: (ACCEPT) ( s ipaddr -- fh ior )
  struct sockaddr addr;  

  fh = accept(s, &addr, sizeof(struct sockaddr));

    

;C
    

    