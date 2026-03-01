'use client';

import { useEffect, useState } from 'react';
import { X } from 'lucide-react';
import { api } from '@/lib/api';
import type { RaidDetailDto } from '@/types/api/RaidDetailDto';
import EncountersAccordion from '@/app/raids/[id]/EncountersAccordion';

interface EncounterModalProps {
  encounterId: number | null;
  onClose: () => void;
}

export default function EncounterModal({ encounterId, onClose }: EncounterModalProps) {
  const [raidData, setRaidData] = useState<RaidDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!encounterId) {
      setRaidData(null);
      setError(null);
      return;
    }

    setIsLoading(true);
    setError(null);

    api.getRaidByEncounterId(encounterId)
      .then((data) => {
        setRaidData(data);
      })
      .catch((err) => {
        setError(err instanceof Error ? err.message : 'Не удалось загрузить данные рейда');
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, [encounterId]);

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        onClose();
      }
    };

    if (encounterId) {
      document.addEventListener('keydown', handleEscape);
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = 'unset';
    };
  }, [encounterId, onClose]);

  if (!encounterId) {
    return null;
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4"
      onClick={onClose}
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-title"
    >
      <div
        className="relative w-full max-w-6xl max-h-[90vh] bg-[var(--background-elevated)] rounded-xl border border-[var(--border-color)] shadow-2xl overflow-hidden flex flex-col"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between px-6 py-4 border-b border-[var(--border-color)] flex-shrink-0 bg-[var(--card-bg)]">
          <h2 id="modal-title" className="text-xl font-bold text-[var(--foreground)]">
            {raidData ? `Рейд: ${raidData.raidTypeName}` : 'Загрузка...'}
          </h2>
          <button
            onClick={onClose}
            className="p-2 rounded-lg hover:bg-[var(--card-hover)] transition-colors text-[var(--foreground-muted)] hover:text-[var(--foreground)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--card-bg)]"
            aria-label="Закрыть модальное окно"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-6 scrollbar-custom">
          {isLoading && (
            <div className="flex items-center justify-center py-20" role="status" aria-live="polite">
              <div className="text-[var(--foreground-muted)]">Загрузка данных рейда...</div>
            </div>
          )}

          {error && (
            <div className="flex items-center justify-center py-20" role="alert">
              <div className="text-[var(--error)]">{error}</div>
            </div>
          )}

          {!isLoading && !error && raidData && (
            <EncountersAccordion encounters={raidData.encounters} />
          )}
        </div>
      </div>
    </div>
  );
}
