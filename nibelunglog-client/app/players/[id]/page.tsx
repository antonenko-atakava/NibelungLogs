"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { playersApi } from "@/utils/api/playersApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { PlayerHeader } from "@/components/players/detail/PlayerHeader";
import { PlayerStats } from "@/components/players/detail/PlayerStats";
import { PlayerSpecsChart } from "@/components/players/detail/PlayerSpecsChart";
import { PlayerEncounterChart } from "@/components/players/detail/PlayerEncounterChart";
import { PlayerEncountersTable } from "@/components/players/detail/PlayerEncountersTable";
import { ErrorMessage } from "@/components/ui/error-message";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { PlayerExtendedDetailDto } from "@/types/api/Player";

export default function PlayerDetailPage() {
  const params = useParams();
  const router = useRouter();
  const playerId = parseInt(params.id as string, 10);

  const [player, setPlayer] = useState<PlayerExtendedDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isNaN(playerId) || playerId <= 0) {
      setError("Неверный ID игрока");
      setIsLoading(false);
      return;
    }

    const fetchPlayer = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await playersApi.getPlayerExtended(playerId);
        if (!result) {
          setError("Игрок не найден");
          setPlayer(null);
        } else {
          setPlayer(result);
          setError(null);
        }
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setPlayer(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchPlayer();
  }, [playerId]);

  if (isLoading)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <div className="flex items-center justify-center min-h-[60vh]">
          <div className="text-muted-foreground text-lg">Загрузка...</div>
        </div>
      </div>
    );

  if (error && !player)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <Button
          variant="ghost"
          onClick={() => router.push("/players")}
          className="mb-6"
        >
          <ArrowLeft className="mr-2 size-4" />
          Назад к списку игроков
        </Button>
        <ErrorMessage message={error} onRetry={() => window.location.reload()} />
      </div>
    );

  if (!player)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <Button
          variant="ghost"
          onClick={() => router.push("/players")}
          className="mb-6"
        >
          <ArrowLeft className="mr-2 size-4" />
          Назад к списку игроков
        </Button>
        <ErrorMessage message="Игрок не найден" onRetry={() => window.location.reload()} />
      </div>
    );

  return (
    <div className="container mx-auto py-12 px-8 max-w-7xl">
      <Button
        variant="ghost"
        onClick={() => router.push("/players")}
        className="mb-6"
      >
        <ArrowLeft className="mr-2 size-4" />
        Назад к списку игроков
      </Button>

      <PlayerHeader player={player} />
      
      <div className="mt-8 grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <PlayerStats player={player} />
      </div>

      {player.specStatistics.length > 0 && (
        <div className="mt-8">
          <PlayerSpecsChart specStatistics={player.specStatistics} playerId={playerId} className={player.className || player.characterClass} />
        </div>
      )}

      <div className="mt-8">
        <PlayerEncounterChart playerId={playerId} />
      </div>

      <div className="mt-8">
        <PlayerEncountersTable playerId={playerId} />
      </div>
    </div>
  );
}
