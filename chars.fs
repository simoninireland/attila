\ $Id$

\ Character model independent character operations

\ Test characters for case-insensitive equality
: C=CI \ ( c1 c2 -- f )
    >UC SWAP >UC C= ;
