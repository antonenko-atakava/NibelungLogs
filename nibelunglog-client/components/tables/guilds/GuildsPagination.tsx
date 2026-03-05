"use client";

import { Button } from "@/components/ui/button";

interface GuildsPaginationProps {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

export function GuildsPagination({
  currentPage,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
}: GuildsPaginationProps) {
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

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 7;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (currentPage <= 3) {
        for (let i = 1; i <= 5; i++) {
          pages.push(i);
        }
        pages.push("...");
        pages.push(totalPages);
      } else if (currentPage >= totalPages - 2) {
        pages.push(1);
        pages.push("...");
        for (let i = totalPages - 4; i <= totalPages; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push("...");
        for (let i = currentPage - 1; i <= currentPage + 1; i++) {
          pages.push(i);
        }
        pages.push("...");
        pages.push(totalPages);
      }
    }

    return pages;
  };

  return (
    <div className="flex items-center justify-between gap-4 px-6 py-4 border-t border-border/30 bg-secondary/10 rounded-b-2xl">
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
        <div className="flex items-center gap-1">
          {getPageNumbers().map((page, index) => {
            if (page === "...") {
              return (
                <span key={`ellipsis-${index}`} className="px-2 text-muted-foreground">
                  ...
                </span>
              );
            }
            const pageNum = page as number;
            return (
              <Button
                key={pageNum}
                variant={currentPage === pageNum ? "default" : "ghost"}
                size="sm"
                onClick={() => onPageChange(pageNum)}
                className={`h-9 min-w-[36px] px-2 font-medium ${
                  currentPage === pageNum ? "" : "hover:bg-secondary/40"
                }`}
              >
                {pageNum}
              </Button>
            );
          })}
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
