"use client";

import { Trophy, Star } from "lucide-react";
import { cn } from "@/lib/utils";

interface RatingBadgeProps {
  rating: number;
  rank?: number;
  className?: string;
}

export function RatingBadge({ rating, rank, className }: RatingBadgeProps) {
  const getRatingColor = (rating: number) => {
    if (rating >= 1000)
      return "text-amber-600/90 dark:text-amber-400/90 border-amber-500/25 bg-amber-500/8";
    if (rating >= 500)
      return "text-purple-600/90 dark:text-purple-400/90 border-purple-500/25 bg-purple-500/8";
    if (rating >= 200)
      return "text-blue-600/90 dark:text-blue-400/90 border-blue-500/25 bg-blue-500/8";
    if (rating >= 100)
      return "text-emerald-600/90 dark:text-emerald-400/90 border-emerald-500/25 bg-emerald-500/8";
    return "text-foreground border-border/40 bg-secondary/20";
  };

  const formatRating = (value: number): string => {
    return new Intl.NumberFormat("ru-RU", {
      maximumFractionDigits: 1,
      minimumFractionDigits: 1,
    }).format(value);
  };

  return (
    <div
      className={cn(
        "inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-md border font-medium",
        getRatingColor(rating),
        className
      )}
    >
      {rank === 1 ? (
        <Trophy className="h-3.5 w-3.5 opacity-80" />
      ) : rank && rank <= 3 ? (
        <Star className="h-3.5 w-3.5 fill-current opacity-80" />
      ) : null}
      <span className="text-sm">{formatRating(rating)}</span>
    </div>
  );
}
