import { PropsWithChildren, useEffect } from 'react';
import { bootstrapAuthSession } from './authBootstrap';

export function AuthBootstrapProvider({ children }: PropsWithChildren) {
  useEffect(() => {
    void bootstrapAuthSession();
  }, []);

  return children;
}
