"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { raidsApi } from "@/utils/api/raidsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { RaidHeader } from "@/components/raids/detail/RaidHeader";
import { RaidStats } from "@/components/raids/detail/RaidStats";
import { RaidEncountersTable } from "@/components/raids/detail/RaidEncountersTable";
import { ErrorMessage } from "@/components/ui/error-message";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { RaidDetailDto } from "@/types/api/Raid";

export default function RaidDetailPage() {
  const params = useParams();
  const router = useRouter();
  const raidId = parseInt(params.id as string, 10);

  const [raid, setRaid] = useState<RaidDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isNaN(raidId) || raidId <= 0) {
      setError("Неверный ID рейда");
      setIsLoading(false);
      return;
    }

    const fetchRaid = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await raidsApi.getRaidById(raidId);
        if (!result) {
          setError("Рейд не найден");
          setRaid(null);
        } else {
          setRaid(result);
          setError(null);
        }
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setRaid(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchRaid();
  }, [raidId]);

  if (isLoading)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <div className="flex items-center justify-center min-h-[60vh]">
          <div className="text-muted-foreground text-lg">Загрузка...</div>
        </div>
      </div>
    );

  if (error && !raid)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <Button
          variant="ghost"
          onClick={() => router.push("/raids")}
          className="mb-6"
        >
          <ArrowLeft className="mr-2 size-4" />
          Назад к списку рейдов
        </Button>
        <ErrorMessage message={error} onRetry={() => window.location.reload()} />
      </div>
    );

  if (!raid)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <Button
          variant="ghost"
          onClick={() => router.push("/raids")}
          className="mb-6"
        >
          <ArrowLeft className="mr-2 size-4" />
          Назад к списку рейдов
        </Button>
        <ErrorMessage message="Рейд не найден" onRetry={() => window.location.reload()} />
      </div>
    );

  return (
    <div className="container mx-auto py-12 px-8 max-w-7xl">
      <Button
        variant="ghost"
        onClick={() => router.push("/raids")}
        className="mb-6"
      >
        <ArrowLeft className="mr-2 size-4" />
        Назад к списку рейдов
      </Button>

      <RaidHeader raid={raid} />
      
      <div className="mt-8 grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <RaidStats raid={raid} />
      </div>

      <div className="mt-8">
        <RaidEncountersTable encounters={raid.encounters} />
      </div>
    </div>
  );
}
