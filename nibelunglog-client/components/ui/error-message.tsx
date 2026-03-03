"use client";

import { AlertCircle } from "lucide-react";
import { Button } from "./button";

interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

export function ErrorMessage({ message, onRetry }: ErrorMessageProps) {
  return (
    <div className="p-6 border border-destructive/40 rounded-2xl bg-destructive/5 shadow-md">
      <div className="flex items-start gap-4">
        <div className="p-2 rounded-lg bg-destructive/10 border border-destructive/20">
          <AlertCircle className="size-5 text-destructive" />
        </div>
        <div className="flex-1">
          <div className="font-semibold mb-2 text-destructive">Ошибка</div>
          <div className="text-sm text-destructive/80 leading-relaxed">{message}</div>
          {onRetry && (
            <Button
              variant="outline"
              size="sm"
              onClick={onRetry}
              className="mt-4 border-destructive/30 text-destructive hover:bg-destructive/10 hover:border-destructive/50 font-medium"
            >
              Попробовать снова
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
