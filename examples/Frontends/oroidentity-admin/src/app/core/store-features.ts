import { signalStoreFeature, withState, withComputed, withMethods } from '@ngrx/signals';
import { computed } from '@angular/core';

export interface LoadingState { loading: boolean; }
export interface ErrorState { error: string | null; }

export function withLoading() {
  return signalStoreFeature(
    withState<LoadingState>({ loading: false }),
    withMethods(store => ({
      setLoading: (loading: boolean) => (store as any)._loading.set(loading),
    }))
  );
}

export function withError() {
  return signalStoreFeature(
    withState<ErrorState>({ error: null }),
    withMethods(store => ({
      setError: (error: string | null) => (store as any)._error.set(error),
      clearError: () => (store as any)._error.set(null),
    }))
  );
}
