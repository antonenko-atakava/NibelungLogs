"use client";

import { Button } from "@/components/ui/button";
import type { PlayerFilters } from "@/types/players/PlayerFilters";

interface PlayersPaginationProps {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

export function PlayersPagination({
  currentPage,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
}: PlayersPaginationProps) {
  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalCount);

  const handlePrevious = () => {
    if (currentPage > 1)
      onPageChange(currentPage - 1);
  };

  const handleNext = () => {
    if (currentPage < totalPages)
      onPageChange(currentPage + 1);
  };

  if (totalPages <= 1)
    return null;

  return (
    <div className="flex flex-col sm:flex-row items-center justify-between gap-4 px-6 py-6 border-t border-border/30 bg-secondary/10">
      <div className="text-sm text-muted-foreground font-light">
        Показано <span className="font-medium text-foreground">{startItem}-{endItem}</span> из{" "}
        <span className="font-medium text-foreground">{totalCount}</span>
      </div>
      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          onClick={handlePrevious}
          disabled={currentPage === 1}
          className="h-9 px-4 font-medium disabled:opacity-30"
        >
          Назад
        </Button>
        <div className="text-sm font-medium px-4 py-1.5 rounded-lg bg-primary/10 text-primary">
          Страница {currentPage} из {totalPages}
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={handleNext}
          disabled={currentPage === totalPages}
          className="h-9 px-4 font-medium disabled:opacity-30"
        >
          Вперед
        </Button>
      </div>
    </div>
  );
}
